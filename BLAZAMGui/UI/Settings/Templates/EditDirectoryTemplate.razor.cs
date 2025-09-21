using MudBlazor;

namespace BLAZAM.Gui.UI.Settings.Templates
{
    public partial class EditDirectoryTemplate : ValidatedForm
    {
        private string? _testFirstName;
        private string? _testMiddleName;
        private string? _testLastName;
        private bool _showOuTree;

        [Parameter]
        public SetSubHeader? Header { get; set; }
        protected DirectoryTemplate originalTemplate;


        AppModal? categoryModal;
        protected string groupText;
        protected List<string> categories = new();
        protected List<TemplateVariable> usernameVariables
        {
            get
            {
                return new List<TemplateVariable>()
                {
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
                };
            }
        }

        private List<DirectoryTemplate> dropdownTemplates = new();
        private DirectoryTemplate _template;

        DirectoryTemplate usernameFromTemplate;
        DirectoryTemplate displayNameFromTemplate;
        DirectoryTemplate passwordFromTemplateName;
        DirectoryTemplate requirePasswordChangeFromTemplate;
        DirectoryTemplate sendWelcomeEmailFromTemplate;
        DirectoryTemplate askForAlternateEmailFromTemplate;


        protected bool fieldDrawerOpen;

        protected List<IActiveDirectoryField> fields = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadData();
        }
        [Parameter]
        public DirectoryTemplate DirectoryTemplate
        {
            get => _template;
            set
            {
                if (_template == value)
                    return;
                if (Context != null && value.Id > 0)
                    value = Context.DirectoryTemplates.First(dt => dt.Id == value.Id);
                _template = value;
                originalTemplate = value;
                SelectedOU = Directory?.OUs.FindOuByDN(value.EffectiveParentOU);

                DirectoryTemplateChanged.InvokeAsync(value);


            }
        }

        [Parameter]
        public EventCallback<DirectoryTemplate> DirectoryTemplateChanged { get; set; }

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
                        existing = new DirectoryTemplateGroup()
                        {
                            GroupSid = group.SID.ToSidString()
                        };
                    DirectoryTemplate.AssignedGroupSids.Add(existing);
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
                usernameFromTemplate = GetParentOfValue<string?>(DirectoryTemplate.EffectiveUsernameFormula, template => template.UsernameFormula);
                displayNameFromTemplate = GetParentOfValue<string?>(DirectoryTemplate.EffectiveDisplayNameFormula, template => template.DisplayNameFormula);
                passwordFromTemplateName = GetParentOfValue<string?>(DirectoryTemplate.EffectivePasswordFormula, template => template.PasswordFormula); ;
                requirePasswordChangeFromTemplate = GetParentOfValue<bool?>(DirectoryTemplate.EffectiveRequirePasswordChange, template => template.RequirePasswordChange); ;
                sendWelcomeEmailFromTemplate = GetParentOfValue<bool?>(DirectoryTemplate.EffectiveSendWelcomeEmail, template => template.SendWelcomeEmail); ;
                askForAlternateEmailFromTemplate = GetParentOfValue<bool?>(DirectoryTemplate.EffectiveAskForAlternateEmail, template => template.AskForAlternateEmail); ;

                fields = await Context.ActiveDirectoryFields.Cast<IActiveDirectoryField>().ToListAsync();
                fields.AddRange(await Context.CustomActiveDirectoryFields.Where(cf => cf.DeletedAt == null).Cast<IActiveDirectoryField>().ToListAsync());

                using (var dropdownContext = await DbFactory.CreateDbContextAsync())
                {
                    dropdownTemplates = await dropdownContext.DirectoryTemplates.Where(t => !t.Equals(DirectoryTemplate) && t.DeletedAt == null).ToListAsync();
                }

                await LoadCategories();
                if (DirectoryTemplate.ParentTemplate is null && DirectoryTemplate.ParentTemplateId > 0)
                {
                    using (var parentContext = await DbFactory.CreateDbContextAsync())
                    {
                        DirectoryTemplate.ParentTemplate = await parentContext.DirectoryTemplates.FirstOrDefaultAsync(t => t.ParentTemplateId.Equals(DirectoryTemplate.ParentTemplateId) && t.DeletedAt == null);

                    }
                }


                if (DirectoryTemplate != null && Context != null && !Context.EntityIsTracked(DirectoryTemplate) == true)
                {
                    var matching = await Context.DirectoryTemplates.Include(dt => dt.ParentTemplate).Where(dt => dt.Id == DirectoryTemplate.Id).FirstOrDefaultAsync();
                    if (matching != null) _template = matching;
                    await LoadParentOU();
                }
                using (var dropdownContext = await DbFactory.CreateDbContextAsync())
                {
                    dropdownTemplates = await dropdownContext.DirectoryTemplates.Where(t => !t.Equals(DirectoryTemplate) && t.DeletedAt == null).ToListAsync();
                }

                RefreshGroups();
                await StateHasChangedAsync();
                Form?.Validate();
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
            var templateCursor = DirectoryTemplate;
            var templateValue = valueSelector.Invoke(templateCursor);
            if (!EqualityComparer<T>.Default.Equals(templateValue, default(T)) && templateValue.Equals(value))
                return null;
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
            if (!DirectoryTemplate.EffectiveParentOU.IsNullOrEmpty())
                SelectedOU = (await Directory.OUs.FindOuByStringAsync(DirectoryTemplate.EffectiveParentOU)).FirstOrDefault();
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
                    if (templateCursor.Equals(DirectoryTemplate))
                    {
                        SnackBarService.Warning("Circular inheritance detected!");
                        return;
                    }
                }

                DirectoryTemplate.ParentTemplate = parent;
                DirectoryTemplate.ParentTemplateId = parent.Id;
            }
            else
            {
                DirectoryTemplate.ParentTemplateId = null;
                DirectoryTemplate.ParentTemplate = null;
            }

            await LoadParentOU();
            Form?.Validate();
        }

        protected IADOrganizationalUnit? SelectedOU;
        protected IDirectoryEntryAdapter? SelectedGroup;
        protected List<IDirectoryEntryAdapter> TemplateGroups = new();
        protected async Task LoadCategories()
        {

            using (var categoryContext = await DbFactory.CreateDbContextAsync())
            {
                categories = await categoryContext.DirectoryTemplates.Select(t => t.Category).Distinct().ToListAsync();
            }

        }


        protected void RefreshGroups()
        {
            TemplateGroups.Clear();
            if (DirectoryTemplate != null)
            {
                foreach (var sid in DirectoryTemplate.AssignedGroupSids)
                {
                    var temp = Directory.Groups.FindGroupBySID(sid.GroupSid);
                    if (temp != null)
                        TemplateGroups.Add(temp);
                }
            }
        }


        private DirectoryTemplateFieldValue GetFieldToEdit(DirectoryTemplateFieldValue fieldValue)
        {
            if (!DirectoryTemplate.FieldValues.Contains(fieldValue))
            {
                DirectoryTemplate.FieldValues.Add((DirectoryTemplateFieldValue)fieldValue.Clone(Context));
            }

            var fieldToModify = DirectoryTemplate.FieldValues.FirstOrDefault(fv => (fv.Field != null && fv.Field.Equals(fieldValue.Field)) || (fv.CustomField != null && fv.CustomField.Equals(fieldValue.CustomField)));
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
            DirectoryTemplate.FieldValues.Remove(field);
            await StateHasChangedAsync();
        }

        private async Task CancelNewTemplate()
        {
            await ClearSelectedTemplate.InvokeAsync();
        }
        private async Task DiscardChanges()
        {
            _template = originalTemplate;
            await StateHasChangedAsync();
        }
        protected async Task SaveTemplate()
        {
            if (Context == null)
                throw new AppException("Database not available");

            DirectoryTemplate.ParentOU = SelectedOU?.DN;

            if (DirectoryTemplate.ParentTemplate != null)
            {
                DirectoryTemplate.ParentTemplate = await Context.DirectoryTemplates
                    .FirstOrDefaultAsync(x => x.Id == DirectoryTemplate.ParentTemplate.Id);
            }

            if (DirectoryTemplate.Id == 0)
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
                foreach (var group in DirectoryTemplate.AssignedGroupSids)
                {
                    trackedGroups.Add(group.Clone(Context));
                }

                foreach (var field in DirectoryTemplate.FieldValues)
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

                DirectoryTemplate.AssignedGroupSids = trackedGroups;
                await Context.DirectoryTemplates.AddAsync(DirectoryTemplate);
                var result = await Context.SaveChangesAsync();

                Header?.OnRefreshRequested?.Invoke();
                if (result > 0)
                {
                    SnackBarService.Success(DirectoryTemplate.Name + " was added.");
                    Nav.NavigateTo($"/templates/{DirectoryTemplate.Id}");
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
                Loggers.DatabaseLogger.Error(ex, "Error attempting to save creation template {@Template}", DirectoryTemplate.Name);
            }
        }

        private async Task UpdateExistingTemplate()
        {
            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
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
                DirectoryTemplate.FieldValues.Add(new DirectoryTemplateFieldValue { Field = adField });
            }
            else if (field is CustomActiveDirectoryField customField)
            {
                DirectoryTemplate.FieldValues.Add(new DirectoryTemplateFieldValue { CustomField = customField });
            }
        }
    }
}