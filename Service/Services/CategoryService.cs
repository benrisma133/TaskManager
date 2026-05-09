using Repository.Models;
using Repository.Repositories;
using Service.Enums.Category;

namespace Service.Services;

public class CategoryService
{
    // ─── enMode ────────────────────────────────────────────────────────────
    public enum enMode { AddNew, Update }
    private enMode _Mode;

    // ─── Properties ────────────────────────────────────────────────────────
    public int CategoryId { get; private set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsCustom { get; private set; }

    // ─── Constructor ───────────────────────────────────────────────────────
    public CategoryService(Category category, enMode mode = enMode.AddNew)
    {
        CategoryId = category.CategoryId;
        Name = category.Name;
        Color = category.Color;
        Icon = category.Icon;
        Type = category.Type;
        IsCustom = category.IsCustom;
        _Mode = mode;
    }

    // ─── Default constructor for AddNew ────────────────────────────────────
    public CategoryService()
    {
        _Mode = enMode.AddNew;
    }

    // ─── Private: Add ──────────────────────────────────────────────────────
    private enCategorySaveResult _AddNew()
    {
        try
        {
            if (CategoryRepository.IsCategoryNameTaken(Name))
                return enCategorySaveResult.DuplicateName;

            var category = new Category
            {
                Name = Name,
                Color = Color,
                Icon = Icon,
                Type = Type
            };

            int newId = CategoryRepository.AddCategory(category);

            if (newId > 0)
            {
                CategoryId = newId;
                _Mode = enMode.Update; // switch mode after successful add
                return enCategorySaveResult.Saved;
            }

            return enCategorySaveResult.Failed;
        }
        catch
        {
            return enCategorySaveResult.Failed;
        }
    }

    // ─── Private: Update ───────────────────────────────────────────────────
    private enCategorySaveResult _Update()
    {
        try
        {
            if (CategoryRepository.IsCategoryNameTakenByOther(CategoryId, Name))
                return enCategorySaveResult.DuplicateName;

            var category = new Category
            {
                CategoryId = CategoryId,
                Name = Name,
                Color = Color,
                Icon = Icon,
                Type = Type
            };

            bool updated = CategoryRepository.UpdateCategory(category);
            return updated ? enCategorySaveResult.Saved : enCategorySaveResult.Failed;
        }
        catch
        {
            return enCategorySaveResult.Failed;
        }
    }

    // ─── Public: Save ──────────────────────────────────────────────────────
    public enCategorySaveResult Save()
    {
        switch (_Mode)
        {
            case enMode.AddNew: return _AddNew();
            case enMode.Update: return _Update();
            default: return enCategorySaveResult.Failed;
        }
    }

    // ─── Static: Delete ────────────────────────────────────────────────────
    public static enCategoryDeleteResult Delete(int categoryId)
    {
        try
        {
            string result = CategoryRepository.DeleteCategory(categoryId);

            return result switch
            {
                "DELETED" => enCategoryDeleteResult.Deleted,
                "NOT_CUSTOM" => enCategoryDeleteResult.NotCustom,
                "HAS_PROJECTS" => enCategoryDeleteResult.HasProjects,
                _ => enCategoryDeleteResult.Failed
            };
        }
        catch
        {
            return enCategoryDeleteResult.Failed;
        }
    }

    // ─── Static: Find ──────────────────────────────────────────────────────
    public static (enCategoryRetrieveResult result, CategoryService? service) Find(int categoryId)
    {
        try
        {
            Category? category = CategoryRepository.GetCategoryById(categoryId);

            if (category is null)
                return (enCategoryRetrieveResult.NotFound, null);

            return (enCategoryRetrieveResult.Found, new CategoryService(category, enMode.Update));
        }
        catch
        {
            return (enCategoryRetrieveResult.Failed, null);
        }
    }

    // ─── Static: GetAll ────────────────────────────────────────────────────
    public static (enCategoryRetrieveResult result, List<Category> categories) GetAll()
    {
        try
        {
            List<Category> list = CategoryRepository.GetAllCategories();
            return (enCategoryRetrieveResult.Found, list);
        }
        catch
        {
            return (enCategoryRetrieveResult.Failed, new List<Category>());
        }
    }

    // ─── Static: IsNameTaken (Add) ─────────────────────────────────────────
    public static bool IsNameTaken(string name)
    {
        try
        {
            return CategoryRepository.IsCategoryNameTaken(name);
        }
        catch
        {
            return false;
        }
    }

    // ─── Static: IsNameTakenByOther (Update) ───────────────────────────────
    public static bool IsNameTakenByOther(int categoryId, string name)
    {
        try
        {
            return CategoryRepository.IsCategoryNameTakenByOther(categoryId, name);
        }
        catch
        {
            return false;
        }
    }
}