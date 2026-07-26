using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.Doc;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.CategoryApp.Dto;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.CategoryApp;

public class CategoryService : ICategoryService, IBaseService
{
    private readonly SmartDocHubDbContext _dbContext;

    public CategoryService(SmartDocHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var res = await _dbContext.Categories.ToListAsync();
        return res;
    } 

    

}
