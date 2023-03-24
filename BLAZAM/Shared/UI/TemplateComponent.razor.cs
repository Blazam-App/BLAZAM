using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Server.Shared.UI.Settings;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Server.Shared.UI
{
    public class TemplateComponent : ValidatedForm
    {
        protected MudTabs? Tabs;
        private IEnumerable<DirectoryTemplate> templates = new List<DirectoryTemplate>();
        private string? selectedCategory;
        private DirectoryTemplate selectedTemplate;

        protected SetHeader? Header { get; set; }


        protected IEnumerable<DirectoryTemplate> Templates
        {
            get
            {
                if (SelectedCategory == null || SelectedCategory == "" || SelectedCategory=="All")
                    return templates;
                else
                    return templates.Where(t => t.Category == SelectedCategory).ToList();

            }
            set => templates = value;
        }
        protected IEnumerable<string?> TemplateCategories { get; private set; }
        protected DirectoryTemplate SelectedTemplate
        {
            get => selectedTemplate; set
            {

                selectedTemplate = value;
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

        protected async Task FetchTemplates()
        {
            if(Context==null) return;
            var temp = await Context.DirectoryTemplates.OrderBy(c => c.Category).OrderBy(c => c.Name).ToListAsync();
            if (temp != null)
                Templates = temp;
            var cats = await Context.DirectoryTemplates.Select(c => c.Category).Where(c=>c!="" && c!=null).Distinct().ToListAsync();
            if (cats != null)
            {
                TemplateCategories = cats;
                TemplateCategories = TemplateCategories.Prepend("All");
                SelectedCategory = TemplateCategories.FirstOrDefault();
            }
            await InvokeAsync(StateHasChanged);
            Header?.OnRefreshRequested?.Invoke();
        }

    }
}
