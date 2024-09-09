using studymate_backend.Enums;
using studymate_backend.Models.StudyMate.Raw.Request.Auth;
using Xunit;

namespace studymate_backend.Tests.Controllers;

public class AuthControllerTests
{
    private static readonly RequestSignUp requestSignUp = new()
    {
        Id = "",
        Password = "",
        PasswordConfirm = "",
        Gender = "",
        NameNick = "",
        NameFirst = "",
        NameLast = ""
    };

    private static readonly RequestSignIn requestSignIn = new()
    {
        Id = "",
        Password = ""
    };

    public static TheoryData<string, string> SignUp_BAD_REQUEST_FIELDS_INVALID_TestCases => new()
    {
        // Invalid ID cases (fields cannot be empty, only numbers, and must be exactly 8 characters)
        { EnumResponseCode.FIELDS_INVALID.GetName(), "" }, // Empty string
        { EnumResponseCode.FIELDS_INVALID.GetName(), "123" }, // Less than 8 characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "123456789" }, // More than 8 characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "1234567a" }, // Contains non-numeric characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "abcd1234" }, // Mix of letters and numbers
        { EnumResponseCode.FIELDS_INVALID.GetName(), " " }, // Single space

        // Valid ID cases (exactly 8 numeric characters)
        { EnumResponseCode.CREATED.GetName(), "12345678" }, // Valid 8 digits
        { EnumResponseCode.CREATED.GetName(), "87654321" } // Another valid 8 digits
    };

    public static TheoryData<string, string, string> SignUp_BAD_REQUEST_PASSWORD_MISMATCH_TestCases => new()
    {
        // Password mismatch cases
        { EnumResponseCode.PASSWORD_MISMATCH.GetName(), "12345678", "1234567" }, // Mismatch by one character
        { EnumResponseCode.PASSWORD_MISMATCH.GetName(), "Password123", "password123" }, // Mismatch by case (uppercase vs lowercase)
        { EnumResponseCode.PASSWORD_MISMATCH.GetName(), "Password@1", "Password@2" }, // Mismatch by special character
        { EnumResponseCode.PASSWORD_MISMATCH.GetName(), "abc123", "def456" }, // Completely different values
        { EnumResponseCode.PASSWORD_MISMATCH.GetName(), "MyPassword!", "MyPassword1" }, // Mismatch by one character at the end

        // Valid password match case
        { EnumResponseCode.CREATED.GetName(), "StrongPassword!1", "StrongPassword!1" } // Exact match
    };

    public static TheoryData<string, string> SignUp_BAD_REQUEST_PASSWORD_WEAK_TestCases => new()
    {
        // Weak password cases
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "1234567" }, // Less than 8 characters
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "abcdefgh" }, // Only lowercase letters, no number or special character
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "ABCDEFGH" }, // Only uppercase letters, no number or special character
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "12345678" }, // Only numbers, no letters or special characters
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "abcdef12" }, // Lowercase letters and numbers, but no special character
        { EnumResponseCode.PASSWORD_WEAK.GetName(), "ABCDEF12" }, // Uppercase letters and numbers, but no special character

        // Valid strong password case
        { EnumResponseCode.CREATED.GetName(), "Abcdef12!" }
    };

    public static TheoryData<string, string> SignUp_NOT_FOUND_DUPLICATE_ID_TestCases => new()
    {
        { EnumResponseCode.DUPLICATE_ID.GetName(), "12345678" } // Duplicate ID
    };

    public static TheoryData<string, string, string> SignIn_BAD_REQUEST_FIELDS_INVALID_TestCases => new()
    {
        // Invalid ID and password cases
        { EnumResponseCode.FIELDS_INVALID.GetName(), "1234567", "ValidPass123!" }, // ID is less than 8 characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "abcdefgh", "ValidPass123!" }, // ID contains non-numeric characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "123456789", "ValidPass123!" }, // ID is more than 8 characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "1234567@", "ValidPass123!" }, // ID contains special characters
        { EnumResponseCode.FIELDS_INVALID.GetName(), "12 34567", "ValidPass123!" }, // ID contains spaces

        // Valid ID and password case
        { EnumResponseCode.CREATED.GetName(), "12345678", "AnyValidPassword1!" } // ID is exactly 8 digits, password can be anything
    };

    [Theory]
    [MemberData(nameof(SignUp_BAD_REQUEST_FIELDS_INVALID_TestCases))]
    public void SignUp_BAD_REQUEST_FIELDS_INVALID(string expectedResult, string id)
    {
        requestSignUp.Id = id;
        requestSignUp.Password = "12345678Z@";
        requestSignUp.PasswordConfirm = "12345678Z@";

        var result = ControllerFactory.GetAuthController().SignUp(requestSignUp);
        Assert.Equal(expectedResult, result.Message);
    }

    [Theory]
    [MemberData(nameof(SignUp_NOT_FOUND_DUPLICATE_ID_TestCases))]
    public void SignUp_NOT_FOUND_DUPLICATE_ID(string expectedResult, string id)
    {
        requestSignUp.Id = id;
        requestSignUp.Password = "12345678Z@";
        requestSignUp.PasswordConfirm = "12345678Z@";

        var controller = ControllerFactory.GetAuthController();

        controller.SignUp(requestSignUp);

        var result = controller.SignUp(requestSignUp);
        Assert.Equal(expectedResult, result.Message);
    }

    [Theory]
    [MemberData(nameof(SignUp_BAD_REQUEST_PASSWORD_MISMATCH_TestCases))]
    public void SignUp_BAD_REQUEST_PASSWORD_MISMATCH(string expectedResult, string password, string passwordConfirm)
    {
        requestSignUp.Id = "12345678";
        requestSignUp.Password = password;
        requestSignUp.PasswordConfirm = passwordConfirm;

        var result = ControllerFactory.GetAuthController().SignUp(requestSignUp);
        Assert.Equal(expectedResult, result.Message);
    }

    [Theory]
    [MemberData(nameof(SignUp_BAD_REQUEST_PASSWORD_WEAK_TestCases))]
    public void SignUp_BAD_REQUEST_PASSWORD_WEAK(string expectedResult, string password)
    {
        requestSignUp.Id = "12345678";
        requestSignUp.Password = password;
        requestSignUp.PasswordConfirm = password;

        var result = ControllerFactory.GetAuthController().SignUp(requestSignUp);
        Assert.Equal(expectedResult, result.Message);
    }

    [Theory]
    [MemberData(nameof(SignIn_BAD_REQUEST_FIELDS_INVALID_TestCases))]
    public void SignIn_BAD_REQUEST_FIELDS_INVALID(string expectedResult, string id, string password)
    {
        requestSignIn.Id = id;
        requestSignIn.Password = password;

        var result = ControllerFactory.GetAuthController().SignIn(requestSignIn);
        Assert.Equal(expectedResult, result.Message);
    }
}