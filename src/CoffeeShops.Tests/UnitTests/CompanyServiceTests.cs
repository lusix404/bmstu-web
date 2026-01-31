using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoffeeShops.Tests.UnitTests.Services
{
    public class CompanyServiceUnitTests
    {
        private readonly ICompanyService _companyService;
        private readonly Mock<ICompanyRepository> _mockCompanyRepository = new();
        private readonly Mock<ILogger<CompanyService>> _mockLogger = new();

        public CompanyServiceUnitTests()
        {
            _companyService = new CompanyService(_mockCompanyRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCompanyById_ReturnsCompany_WhenCompanyExists()
        {
           
            var companyId = Guid.NewGuid();
            var roleId = 1;
            var expectedCompany = new Company { Id_company = companyId, Name = "Test Company" };

            _mockCompanyRepository.Setup(repo => repo.GetCompanyByIdAsync(companyId, roleId))
                .ReturnsAsync(expectedCompany);

           
            var result = await _companyService.GetCompanyByIdAsync(companyId, roleId);

            
            Assert.Equal(expectedCompany, result);
            _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId, roleId), Times.Once);
        }

        [Fact]
        public async void GetCompanyById_ThrowsCompanyNotFoundException_WhenCompanyDoesNotExist()
        {
           
            var companyId = Guid.NewGuid();
            var roleId = 1;

            _mockCompanyRepository.Setup(repo => repo.GetCompanyByIdAsync(companyId, roleId))
                .ReturnsAsync((Company?)null);

            
            await Assert.ThrowsAsync<CompanyNotFoundException>(
                () => _companyService.GetCompanyByIdAsync(companyId, roleId));
        }

        [Fact]
        public async void GetAllCompanies_ReturnsCompanies_WhenCompaniesExist()
        {
           
            var roleId = 1;
            var expectedCompanies = new List<Company>
            {
                new Company { Id_company = Guid.NewGuid(), Name = "Company 1" },
                new Company { Id_company = Guid.NewGuid(), Name = "Company 2" }
            };

            _mockCompanyRepository.Setup(repo => repo.GetAllCompaniesAsync(roleId))
                .ReturnsAsync(expectedCompanies);

           
            var result = await _companyService.GetAllCompaniesAsync(roleId);

            
            Assert.Equal(expectedCompanies, result);
            Assert.Equal(2, result?.Count);
        }

        [Fact]
        public async void GetAllCompanies_ThrowsNoCompaniesFoundException_WhenNoCompaniesExist()
        {
           
            var roleId = 1;
            var emptyCompaniesList = new List<Company>();

            _mockCompanyRepository.Setup(repo => repo.GetAllCompaniesAsync(roleId))
                .ReturnsAsync(emptyCompaniesList);

            
            await Assert.ThrowsAsync<NoCompaniesFoundException>(
                () => _companyService.GetAllCompaniesAsync(roleId));
        }

        [Fact]
        public async void GetAllCompanies_ThrowsNoCompaniesFoundException_WhenRepositoryReturnsNull()
        {
           
            var roleId = 1;

            _mockCompanyRepository.Setup(repo => repo.GetAllCompaniesAsync(roleId))
                .ReturnsAsync((List<Company>?)null);

            
            await Assert.ThrowsAsync<NoCompaniesFoundException>(
                () => _companyService.GetAllCompaniesAsync(roleId));
        }
    }
}