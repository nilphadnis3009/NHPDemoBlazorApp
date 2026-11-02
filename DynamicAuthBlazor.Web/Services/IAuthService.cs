using Entities.UIModels;

namespace DynamicAuthBlazor.Web.Services
{
        public interface IAuthService
        {
            Task<LoginResultDto> LoginAsync(LoginModel loginModel);
            Task LogoutAsync();
        }
   
}
