using MudBlazor;

namespace BLAZAM.Gui.UI.Settings.Templates
{
    public partial class EditDirectoryTemplate : ValidatedForm
    {
        private string? _testFirstName;
        private string? _testMiddleName;
        private string? _testLastName;
        private bool _showOuTree;
        private AppModal? _categoryModal;
        private List<DirectoryTemplate> _dropdownTemplates = [];
        private DirectoryTemplate _template;
        private DirectoryTemplate _workingTemplate = new();

        private DirectoryTemplate _usernameFromTemplate;
        private DirectoryTemplate _displayNameFromTemplate;
        private DirectoryTemplate _passwordFromTemplateName;
        private DirectoryTemplate _requirePasswordChangeFromTemplate;
        private DirectoryTemplate _sendWelcomeEmailFromTemplate;
        private DirectoryTemplate _askForAlternateEmailFromTemplate;


        [Parameter]
        public SetSubHeader? Header { get; set; }


        protected string groupText;
        protected List<string> categories = [];
        protected List<TemplateVariable> UsernameVariables
        {
            get
            {
                return
                [
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.First_Name],
                        Value = "{fn}"
                    },
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.First_Initial],
                        Value = "{fi}"
                    },
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.Middle_Name],
                        Value = "{mn}"
                    },
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.Middle_Initial],
                        Value = "{mi}"
                    },
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.Last_Name],
                        Value = "{ln}"
                    },
                    new TemplateVariable()
                    {
                        DisplayName = AppLocalization[Lang.Last_Initial],
                        Value = "{li}"
                    },
                ];
            }
        }

       


        protected bool fieldDrawerOpen;

        protected List<IActiveDirectoryField> fields = [];

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
        [Parameter]
        public DirectoryTemplate DirectoryTemplate
        {
            get;
            set;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (_template == DirectoryTemplate)
            {
                return;
            }

            _template = DirectoryTemplate;
            await LoadData();
        }

        [Parameter]
        public EventCallback ClearSelectedTemplate { get; set; }

        protected async Task AssignGroup()
        {
            try
            {
                var group = SelectedGroup as IADGroup;
                if (group != null && group.SID != null && Context != null)
                {
                    var existing = await Context.DirectoryTemplateGroups.Where(g => g.GroupSid == group.SID.ToSidString()).FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        existing = new DirectoryTemplateGroup()
                        {
                            GroupSid = group.SID.ToSidString()
                        };
                    }

                    _workingTemplate?.AssignedGroupSids.Add(existing);
                    SelectedGroup = null;
                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error while assigning group to template");
                SnackBarService.Error(ex.Message);

            }


        }

        protected async Task LoadData()
        {
            if (Context != null)
            {
                if (DirectoryTemplate.Id > 0)
                {
                    _workingTemplate = await Context.DirectoryTemplates.LoadTemplateWithParents(DirectoryTemplate?.Id);
                }
                else
                {
                    _workingTemplate = DirectoryTemplate;
                }

                SelectedOU = Directory?.OUs.FindOuByDN(_workingTemplate.EffectiveParentOU);

                _usernameFromTemplate = GetParentOfValue<string?>(_workingTemplate.EffectiveUsernameFormula, template => template.UsernameFormula);
                _displayNameFromTemplate = GetParentOfValue<string?>(_workingTemplate.EffectiveDisplayNameFormula, template => template.DisplayNameFormula);
                _passwordFromTemplateName = GetParentOfValue<string?>(_workingTemplate.EffectivePasswordFormula, template => template.PasswordFormula);
                _requirePasswordChangeFromTemplate = GetParentOfValue<bool?>(_workingTemplate.EffectiveRequirePasswordChange, template => template.RequirePasswordChange);
                _sendWelcomeEmailFromTemplate = GetParentOfValue<bool?>(_workingTemplate.EffectiveSendWelcomeEmail, template => template.SendWelcomeEmail);
                _askForAlternateEmailFromTemplate = GetParentOfValue<bool?>(_workingTemplate.EffectiveAskForAlternateEmail, template => template.AskForAlternateEmail); 

                fields = await Context.ActiveDirectoryFields.Cast<IActiveDirectoryField>().ToListAsync();
                fields.AddRange(await Context.CustomActiveDirectoryFields.Where(cf => cf.DeletedAt == null).Cast<IActiveDirectoryField>().ToListAsync());
                
                using (var dropdownContext = await DbFactory.CreateDbContextAsync())
                {
                    _dropdownTemplates = await dropdownContext.DirectoryTemplates.Where(t => !t.Equals(_workingTemplate) && t.DeletedAt == null).ToListAsync();
                }

                await LoadCategories();


                if (_workingTemplate != null && Context != null && !Context.EntityIsTracked(_workingTemplate))
                {
                    await LoadParentOU();
                }
                
                RefreshGroups();
                await StateHasChangedAsync();
                if (Form != null)
                {
                    await Form.ValidateAsync();
                }
            }

        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueSelector"></param>
        /// <returns>Null if the value comes from the current
        ///  template, otherwise the source template</returns>
        private DirectoryTemplate? GetParentOfValue<T>(T? value, Func<DirectoryTemplate, T?> valueSelector)
        {
            var templateCursor = _workingTemplate;
            var templateValue = valueSelector.Invoke(templateCursor);
            if (!EqualityComparer<T>.Default.Equals(templateValue, default(T)) && templateValue.Equals(value))
            {
                return null;
            }

            while (templateCursor?.ParentTemplateId != null)
            {
                if (templateCursor.ParentTemplate == null)
                {
                    templateCursor.ParentTemplate = Context.DirectoryTemplates.FirstOrDefault(t => t.Id == templateCursor.ParentTemplateId);
                }
                templateCursor = templateCursor.ParentTemplate;
                if (templateCursor != null)
                {
                    var templateCursorValue = valueSelector.Invoke(templateCursor);
                    if (!EqualityComparer<T?>.Default.Equals(templateCursorValue, default(T?)) && templateCursorValue.Equals(value))
                    {
                        return templateCursor;

                    }
                }
            }

            return null;
        }
        private async Task LoadParentOU()
        {
            if (!_workingTemplate.EffectiveParentOU.IsNullOrEmpty())
            {
                SelectedOU = (await Directory.OUs.FindOuByStringAsync(_workingTemplate.EffectiveParentOU)).FirstOrDefault();
            }

            if (SelectedOU == null)
            {
                SelectedOU = Directory.OUs.GetApplicationRootOU();
            }
        }



        protected async Task ParentTemplateChanged(DirectoryTemplate? parent)
        {
            if (parent != null)
            {
                var templateCursor = parent;
                while (templateCursor.ParentTemplate != null)
                {
                    templateCursor = templateCursor.ParentTemplate;
                    if (templateCursor.Id.Equals(_workingTemplate.Id))
                    {
                        SnackBarService.Warning("Circular inheritance detected!");
                        return;
                    }
                }

                _workingTemplate.ParentTemplate = parent;
                _workingTemplate.ParentTemplateId = parent.Id;
            }
            else
            {
                _workingTemplate.ParentTemplateId = null;
                _workingTemplate.ParentTemplate = null;
            }

            await LoadParentOU();
            if (Form != null)
            {
                await Form.ValidateAsync();
            }
        }

        protected IADOrganizationalUnit? SelectedOU;
        protected IDirectoryEntryAdapter? SelectedGroup;
        protected List<IDirectoryEntryAdapter> TemplateGroups = [];
        protected async Task LoadCategories()
        {

            using var categoryContext = await DbFactory.CreateDbContextAsync();
            categories = await categoryContext.DirectoryTemplates.Select(t => t.Category).Distinct().ToListAsync();
            categories.Sort();

        }


        protected void RefreshGroups()
        {
            TemplateGroups.Clear();
            if (_workingTemplate != null)
            {
                foreach (var sid in _workingTemplate.AssignedGroupSids)
                {
                    var temp = Directory.Groups.FindGroupBySID(sid.GroupSid);
                    if (temp != null)
                    {
                        TemplateGroups.Add(temp);
                    }
                }
            }
        }


        private DirectoryTemplateFieldValue GetFieldToEdit(DirectoryTemplateFieldValue fieldValue)
        {
            if (!_workingTemplate.FieldValues.Contains(fieldValue))
            {
                _workingTemplate.FieldValues.Add((DirectoryTemplateFieldValue)fieldValue.Clone(Context));
            }

            var fieldToModify = _workingTemplate.FieldValues.FirstOrDefault(fv => (fv.Field != null && fv.Field.Equals(fieldValue.Field)) || (fv.CustomField != null && fv.CustomField.Equals(fieldValue.CustomField)));
            return fieldToModify;
        }




        protected void ValueChanged(string? newValue, DirectoryTemplateFieldValue fieldValue)
        {
            var fieldToModify = GetFieldToEdit(fieldValue);
            fieldToModify.Value = newValue;
        }
        protected void EditableChanged(bool newValue, DirectoryTemplateFieldValue fieldValue)
        {
            var fieldToModify = GetFieldToEdit(fieldValue);
            fieldToModify.Editable = newValue;
        }

        protected void RequiredChanged(bool newValue, DirectoryTemplateFieldValue fieldValue)
        {
            var fieldToModify = GetFieldToEdit(fieldValue);
            fieldToModify.Required = newValue;
        }
        protected async Task RemoveField(DirectoryTemplateFieldValue field)
        {
            _workingTemplate.FieldValues.Remove(field);
            await StateHasChangedAsync();
        }

        private async Task CancelNewTemplate()
        {
            await ClearSelectedTemplate.InvokeAsync();
        }
        protected async Task SaveTemplate()
        {
            if (Context == null)
            {
                throw new AppException("Database not available");
            }

            _workingTemplate.ParentOU = SelectedOU?.DN;

            if (_workingTemplate.ParentTemplate != null)
            {
                _workingTemplate.ParentTemplate = await Context.DirectoryTemplates
                    .FirstOrDefaultAsync(x => x.Id == _workingTemplate.ParentTemplate.Id);
            }

            if (_workingTemplate.Id == 0)
            {
                await AddNewTemplate();
            }
            else
            {
                await UpdateExistingTemplate();
            }
        }

        private async Task AddNewTemplate()
        {
            try
            {
                var trackedGroups = new List<DirectoryTemplateGroup>();
                foreach (var group in _workingTemplate.AssignedGroupSids)
                {
                    trackedGroups.Add(group.Clone(Context));
                }

                foreach (var field in _workingTemplate.FieldValues)
                {
                    if (field.Field != null)
                    {
                        field.Field = await Context.ActiveDirectoryFields.FirstOrDefaultAsync(f => f.Id == field.Field.Id);
                    }
                    else if (field.CustomField != null)
                    {
                        field.CustomField = await Context.CustomActiveDirectoryFields.FirstOrDefaultAsync(f => f.Id == field.CustomField.Id);
                    }
                }

                _workingTemplate.AssignedGroupSids = trackedGroups;
                await Context.DirectoryTemplates.AddAsync(_workingTemplate);
                var result = await Context.SaveChangesAsync();
                Analytics.DirectoryTemplateCreated(_workingTemplate.Name);
                Header?.OnRefreshRequested?.Invoke();
                if (result > 0)
                {
                    SnackBarService.Success(_workingTemplate.Name + " was added.");
                    Nav.NavigateTo($"/templates/{_workingTemplate.Id}");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.HResult == -2146232060)
                {
                    SnackBarService.Error("Each template must be uniquely named.");
                }
            }
            catch (SqlException ex)
            {
                Loggers.DatabaseLogger.Error(ex, "Error attempting to save creation template {@Template}", _workingTemplate.Name);
            }
        }

        private async Task UpdateExistingTemplate()
        {
            if (Context.EntityIsTracked(_workingTemplate))
            {
                Loggers.SystemLogger.Debug("Directory Template is tracked for updating");
            }
            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                Analytics.DirectoryTemplateEdited(_workingTemplate.Name);
                SnackBarService.Success("Template changes saved");
            }
            else
            {
                SnackBarService.Warning("No changes");
            }
        }
        public class TemplateVariable
        {
            public string? DisplayName { get; set; }

            public string? Value { get; set; }
        }
        private void AddField(IActiveDirectoryField field)
        {
            if (field is ActiveDirectoryField adField)
            {
                _workingTemplate.FieldValues.Add(new DirectoryTemplateFieldValue { Field = adField });
            }
            else if (field is CustomActiveDirectoryField customField)
            {
                _workingTemplate.FieldValues.Add(new DirectoryTemplateFieldValue { CustomField = customField });
            }
        }
    }
}