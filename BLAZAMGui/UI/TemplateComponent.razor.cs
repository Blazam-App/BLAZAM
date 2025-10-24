
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Gui.UI.Settings;
using MudBlazor;

namespace BLAZAM.Gui.UI
{
    public abstract class TemplateComponent : ValidatedForm
    {
        protected MudTabs? Tabs;
        private IEnumerable<DirectoryTemplate> templates = [];
        private string? selectedCategory;
        private DirectoryTemplate? selectedTemplate;

        protected SetSubHeader? Header { get; set; }




        protected IEnumerable<DirectoryTemplate> Templates
        {
            get
            {
                if (SelectedCategory == null || SelectedCategory == "" || SelectedCategory == "All")
                {
                    return templates;
                }
                else
                {
                    return templates.Where(t => t.Category == SelectedCategory);
                }
            }
            set => templates = value;
        }

        public IEnumerable<DirectoryTemplate> TemplatesUserCanUse
        {
            get
            {
                var list = new List<DirectoryTemplate>();
                foreach (var template in Templates)
                {
                    if (CurrentUser.State.HasActionPermission(template.ParentOU, ObjectActions.Create, ActiveDirectoryObjectType.User))
                    {
                        list.Add(template);

                    }

                }
                return list;
            }
        }
        protected IEnumerable<string?> TemplateCategories { get; private set; }
        protected IEnumerable<string?> TemplateCategoriesUserCanUse
        {
            get
            {
                var cats = TemplatesUserCanUse.Select(c => c.Category).Where(c => c != "" && c != null).Distinct().ToList();
                return cats;
            }
        }

        public DirectoryTemplate? SelectedTemplate
        {
            get => selectedTemplate; set
            {
                if (selectedTemplate == value)
                {
                    return;
                }

                selectedTemplate = value;

                _templateIdParameter = value?.Id;
                Header?.OnRefreshRequested?.Invoke();

            }

        }
        protected string? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                selectedCategory = value;
                Header?.OnRefreshRequested?.Invoke();
            }
        }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await FetchTemplates();
        }
        protected async Task RefreshComponents()
        {
            await StateHasChangedAsync();
            Header?.OnRefreshRequested?.Invoke();
        }




        private int? _templateIdParameter;
        [Parameter]
        public int? TemplateIdParameter
        {
            get => _templateIdParameter;
            set
            {
                _templateIdParameter = value;
                if (value == null || value > 0)
                {
                    var cachedTemplate = Templates.FirstOrDefault(t => t.Id == value);
                    if (cachedTemplate != null)
                    {
                        SelectedTemplate = cachedTemplate;
                    }
                }
                else if (value == 0)
                {
                    SelectedTemplate = new();
                }

            }
        }


        protected async Task FetchTemplates()
        {
            try
            {
                var temp = await Context.DirectoryTemplates.Include(t => t.ParentTemplate).OrderBy(c => c.Category).OrderBy(c => c.Name).ToListAsync();
                if (temp != null)
                {
                    Templates = temp;
                }

                var cats = await Context.DirectoryTemplates.Select(c => c.Category).Where(c => c != "" && c != null).Distinct().ToListAsync();
                if (cats != null)
                {
                    TemplateCategories = cats;
                    TemplateCategories = TemplateCategories.Prepend("All");
                    SelectedCategory = TemplateCategories.FirstOrDefault();
                }
                if (TemplateIdParameter != 0)
                {
                    SelectedTemplate = Templates.FirstOrDefault(t => t.Id == TemplateIdParameter);
                }
                await StateHasChangedAsync();
                Header?.OnRefreshRequested?.Invoke();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error fetching templates {Error}", ex);
            }
        }

    }
}
