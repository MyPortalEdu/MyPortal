﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Enums;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Authentication;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Extensions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Settings;
using MyPortal.Logic.Models.Requests.Auth;
using MyPortal.Logic.Models.Requests.Settings.Users;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Services
{
    public sealed class UserService : BaseService, IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserClaimsPrincipalFactory<User> _claimsPrincipalFactory;

        public UserService(ISessionUser user, UserManager<User> userManager, RoleManager<Role> roleManager,
            SignInManager<User> signInManager, IUserClaimsPrincipalFactory<User> claimsPrincipalFactory) : base(user)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _claimsPrincipalFactory = claimsPrincipalFactory;
        }
        
        private async Task<bool> VerifyUserAccess(Guid requestedUserId, bool edit)
        {
            // Users do not require extra permission to access resources related to themselves
            return await User.HasPermission(this, edit
                       ? PermissionValue.SystemEditUsers
                       : PermissionValue.SystemViewUsers) ||
                   User.GetUserId() == requestedUserId;
        }

        public async Task<bool> UserHasPermission(Guid userId, PermissionRequirement requirement,
            params PermissionValue[] permissionValues)
        {
            var userPermValues = (await GetPermissionValuesByUser(userId)).ToList();

            foreach (var permissionValue in permissionValues)
            {
                if (userPermValues.Contains((int)permissionValue))
                {
                    if (requirement == PermissionRequirement.RequireAny)
                    {
                        return true;
                    }
                }
                else if (requirement == PermissionRequirement.RequireAll)
                {
                    return false;
                }
            }

            return requirement == PermissionRequirement.RequireAll;
        }

        public async Task<IEnumerable<int>> GetPermissionValuesByUser(Guid userId)
        {
            await using var unitOfWork = await User.GetConnection();

            var rolePermissions = (await unitOfWork.UserRoles.GetByUser(userId)).ToList();

            BitArray userPermissions = null;

            foreach (var role in rolePermissions)
            {
                var permissions = new BitArray(role.Role.Permissions);

                userPermissions = userPermissions == null ? permissions : userPermissions.And(permissions);
            }

            var permIndexes = Enumerable.Range(0, Enum.GetNames(typeof(PermissionValue)).Length);

            List<int> permissionValues = new List<int>();

            if (userPermissions != null)
            {
                foreach (var permIndex in permIndexes)
                {
                    if (userPermissions[permIndex])
                    {
                        permissionValues.Add(permIndex);
                    }
                }
            }

            return permissionValues;
        }

        public async Task<UserInfoModel> GetUserInfo()
        {
            var userId = User.GetUserId();

            if (userId != null)
            {
                return await GetUserInfo(userId.Value);
            }

            throw Unauthenticated();
        }

        public async Task<UserInfoModel> GetUserInfo(Guid userId)
        {
            await using var unitOfWork = await User.GetConnection();

            var response = new UserInfoModel();

            var user = await unitOfWork.Users.GetById(userId);
            var userModel = new UserModel(user);

            response.DisplayName = userModel.GetDisplayName(NameFormat.FullNameNoTitle, true, false);
            response.ProfileImage = await userModel.GetProfileImageAsBase64(unitOfWork);
            response.Permissions = (await GetPermissionValuesByUser(userId)).ToArray();

            return response;
        }

        public async Task<IEnumerable<UserModel>> GetUsers(string usernameSearch)
        {
            var query = _userManager.Users;

            if (!string.IsNullOrWhiteSpace(usernameSearch))
            {
                query = query.Where(u => u.UserName.StartsWith(usernameSearch));
            }

            var users = await query.ToListAsync();

            return users.OrderBy(u => u.UserName).Select(u => new UserModel(u));
        }

        public async Task<IEnumerable<Guid>> CreateUser(UserRequestModel request)
        {
            var newIds = new List<Guid>();
            if (await UsernameExists(request.Username))
            {
                throw new LogicException("The username is already in use.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Username.ToLower(),
                UserType = request.UserType,
                PersonId = request.PersonId,
                Enabled = true,
                CreatedDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b.Description}"));
            }

            newIds.Add(user.Id);

            return newIds.ToArray();
        }

        public async Task LinkPerson(Guid userId, Guid personId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.PersonId = personId;

            await _userManager.UpdateAsync(user);
        }

        public async Task UnlinkPerson(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.PersonId = null;

            await _userManager.UpdateAsync(user);
        }

        public async Task UpdateUser(Guid userId, UserRequestModel updateUserRequest)
        {
            Validate(updateUserRequest);

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.UserName = updateUserRequest.Username;
            user.PersonId = updateUserRequest.PersonId;

            await _userManager.UpdateAsync(user);

            var selectedRoles = new List<Role>();

            var existingRoleNames = await _userManager.GetRolesAsync(user);

            foreach (var roleId in updateUserRequest.RoleIds)
            {
                selectedRoles.Add(await _roleManager.FindByIdAsync(roleId.ToString()));
            }

            var rolesToRemove = existingRoleNames.Where(r => selectedRoles.All(s => s.Name != r));

            var rolesToAdd = selectedRoles.Where(s => existingRoleNames.All(r => r != s.Name)).Select(x => x.Name);

            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        public async Task DeleteUser(Guid userId)
        {
            await using var unitOfWork = await User.GetConnection();

            var userRoles = await unitOfWork.UserRoles.GetByUser(userId);

            await RemoveFromRoles(userId, userRoles.Select(ur => ur.RoleId).ToArray());

            var user = await _userManager.FindByIdAsync(userId.ToString());

            await _userManager.DeleteAsync(user);
        }

        public async Task SetPassword(Guid userId, string newPassword)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            List<IdentityError> errors = new List<IdentityError>();

            foreach (var passwordValidator in _userManager.PasswordValidators)
            {
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, newPassword);
                if (!passwordValidationResult.Succeeded)
                {
                    errors.AddRange(passwordValidationResult.Errors);
                }
            }

            if (errors.Any())
            {
                var message = errors.Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b.Description}");
                throw new Exception(message);
            }

            await _userManager.RemovePasswordAsync(user);

            var setPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);

            if (!setPasswordResult.Succeeded)
            {
                errors.AddRange(setPasswordResult.Errors);
            }

            if (errors.Any())
            {
                var message = errors.Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b.Description}");
                throw new Exception(message);
            }
        }

        public async Task ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            await VerifyUserAccess(userId, true);
            
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            List<IdentityError> errors = new List<IdentityError>();

            foreach (var passwordValidator in _userManager.PasswordValidators)
            {
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, newPassword);
                if (!passwordValidationResult.Succeeded)
                {
                    errors.AddRange(passwordValidationResult.Errors);
                }
            }

            if (errors.Any())
            {
                var message = errors.Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b.Description}");
                throw new Exception(message);
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
            }

            if (errors.Any())
            {
                var message = errors.Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b.Description}");
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Login method to use for 3rd party logins.
        /// MyPortal Web App and Mobile App(s) should use the API /connect/token endpoint to authenticate.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<LoginResult> Login(LoginRequestModel login)
        {
            var result = new LoginResult();

            var user = await _userManager.Users.Include(u => u.Person)
                .FirstOrDefaultAsync(x => x.UserName == login.Username.ToLower());

            if (user == null)
            {
                result.Fail("Username/password incorrect.");

                return result;
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);

            if (!signInResult.Succeeded)
            {
                result.Fail("Username/password incorrect.");
            }
            else if (!user.Enabled)
            {
                result.Fail("Your account is currently disabled. Please try again later.");
            }
            else
            {
                var principal = await _claimsPrincipalFactory.CreateAsync(user);
                result.Success(principal);
            }

            return result;
        }

        public async Task AddToRoles(Guid userId, params Guid[] roleIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            foreach (var roleId in roleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());

                await _userManager.AddToRoleAsync(user, role.Name);
            }
        }

        public async Task RemoveFromRoles(Guid userId, params Guid[] roleIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            foreach (var roleId in roleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());

                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }

        public async Task<UserModel> GetCurrentUser()
        {
            var userId = User.GetUserId();

            if (userId != null)
            {
                await using var unitOfWork = await User.GetConnection();
                var user = await unitOfWork.Users.GetById(userId.Value);
                
                var userModel = new UserModel(user);
                
                return userModel;
            }
            else
            {
                throw Unauthenticated();
            }
        }

        public async Task<IEnumerable<RoleModel>> GetUserRoles(Guid userId)
        {
            await VerifyUserAccess(userId, false);
            
            var user = await _userManager.FindByIdAsync(userId.ToString());

            var roleNames = await _userManager.GetRolesAsync(user);

            var roles = new List<RoleModel>();

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);

                roles.Add(new RoleModel(role));
            }

            return roles;
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }

        public async Task SetUserEnabled(Guid userId, bool enabled)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.Enabled = enabled;

            await _userManager.UpdateAsync(user);
        }

        public async Task<UserModel> GetUserById(Guid userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (user.PersonId.HasValue)
            {
                await using var unitOfWork = await User.GetConnection();

                user.Person = await unitOfWork.People.GetById(user.PersonId.Value);
            }

            return new UserModel(user);
        }

        public async Task<UserModel> GetUserByPrincipal(ClaimsPrincipal principal)
        {
            var userId = principal.GetUserId();

            if (userId != null)
            {
                return await GetUserById(userId.Value);
            }

            throw Unauthenticated();
        }
    }
}