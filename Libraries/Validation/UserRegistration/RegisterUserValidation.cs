using Models.UserAuthentication;
using System;

namespace Validation.UserRegistration
{
    public class RegisterUserValidation : ValidationRegex
    {
        public bool IsRegisterUserModelValid(RegisterUser model)
        {
            return (this.IsUserNameValid(model.username) && this.IsPasswordValid(model.password) && this.IsEmailValid(model.email));
        }

        public bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Must provide an email to IsEmailValid function");
            }

            return this._emailRegex.IsMatch(email);
        }
        public bool IsUserNameValid(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("Must provide an Username to IsUserName function");
            }

            return this._userRegex.IsMatch(userName);
        }

        public bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("Must provide an password to IsPassword function");
            }

            return this._passwordRegex.IsMatch(password);
        }
    }
}