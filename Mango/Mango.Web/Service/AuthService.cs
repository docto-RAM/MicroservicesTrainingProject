﻿using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registerRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = registerRequestDto,
                Url = WebSD.APIBase.AuthAPI + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = loginRequestDto,
                Url = WebSD.APIBase.AuthAPI + "/api/auth/login"
            }, withBearer: false);
        }

        public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = registrationRequestDto,
                Url = WebSD.APIBase.AuthAPI + "/api/auth/register"
            }, withBearer: false);
        }
    }
}
