namespace TicketManagement.Domain.Common;

/// <summary>
/// ?? BIG TECH LEVEL: Centralized domain errors for consistent error handling
/// All domain-specific errors are defined here for reusability and consistency
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Ticket-related domain errors
    /// </summary>
    public static class Ticket
    {
        public static Error NotFound(int ticketId) =>
            Error.NotFound("Ticket.NotFound", $"Ticket with ID '{ticketId}' was not found.");

        public static readonly Error AlreadyClosed =
            Error.Validation("Ticket.AlreadyClosed", "Ticket is already closed.");

        public static readonly Error AlreadyAssigned =
            Error.Validation("Ticket.AlreadyAssigned", "Ticket is already assigned.");

        public static readonly Error NotAssigned =
            Error.Validation("Ticket.NotAssigned", "Ticket is not assigned to any agent.");

        public static readonly Error CannotUpdateClosed =
            Error.Validation("Ticket.CannotUpdateClosed", "Cannot update a closed ticket.");

        public static readonly Error CannotAssignClosed =
            Error.Validation("Ticket.CannotAssignClosed", "Cannot assign a closed ticket.");

        public static readonly Error CannotCloseClosed =
            Error.Validation("Ticket.CannotCloseClosed", "Cannot close a ticket that is already closed.");

        public static readonly Error CannotReopenNotClosed =
            Error.Validation("Ticket.CannotReopenNotClosed", "Only closed tickets can be reopened.");

        public static readonly Error InvalidTitle =
            Error.Validation("Ticket.InvalidTitle", "Ticket title cannot be empty or exceed 200 characters.");

        public static readonly Error InvalidDescription =
            Error.Validation("Ticket.InvalidDescription", "Ticket description cannot be empty or exceed 5000 characters.");

        public static readonly Error InvalidCategoryId =
            Error.Validation("Ticket.InvalidCategoryId", "Invalid category ID.");

        public static readonly Error InvalidCreatorId =
            Error.Validation("Ticket.InvalidCreatorId", "Invalid creator ID.");

        public static readonly Error InvalidAgentId =
            Error.Validation("Ticket.InvalidAgentId", "Invalid agent ID.");

        public static readonly Error CannotResolveUnassigned =
            Error.Validation("Ticket.CannotResolveUnassigned", "Only assigned tickets can be resolved.");

        public static readonly Error InvalidStatusTransition =
            Error.Validation("Ticket.InvalidStatusTransition", "Invalid status transition.");
    }

    /// <summary>
    /// User-related domain errors
    /// </summary>
    public static class User
    {
        public static Error NotFound(int userId) =>
            Error.NotFound("User.NotFound", $"User with ID '{userId}' was not found.");

        public static Error EmailAlreadyExists(string email) =>
            Error.Conflict("User.EmailAlreadyExists", $"User with email '{email}' already exists.");

        public static readonly Error InvalidEmail =
            Error.Validation("User.InvalidEmail", "Invalid email format.");

        public static readonly Error InvalidPassword =
            Error.Validation("User.InvalidPassword", "Password does not meet requirements.");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("User.InvalidCredentials", "Invalid email or password.");

        public static readonly Error AccountDeactivated =
            Error.Forbidden("User.AccountDeactivated", "User account is deactivated.");

        public static readonly Error InsufficientPermissions =
            Error.Forbidden("User.InsufficientPermissions", "You don't have permission to perform this action.");

        public static readonly Error InvalidFirstName =
            Error.Validation("User.InvalidFirstName", "First name cannot be empty or exceed 100 characters.");

        public static readonly Error InvalidLastName =
            Error.Validation("User.InvalidLastName", "Last name cannot be empty or exceed 100 characters.");
    }

    /// <summary>
    /// Category-related domain errors
    /// </summary>
    public static class Category
    {
        public static Error NotFound(int categoryId) =>
            Error.NotFound("Category.NotFound", $"Category with ID '{categoryId}' was not found.");

        public static Error NameAlreadyExists(string name) =>
            Error.Conflict("Category.NameAlreadyExists", $"Category with name '{name}' already exists.");

        public static readonly Error InvalidName =
            Error.Validation("Category.InvalidName", "Category name cannot be empty or exceed 100 characters.");
    }

    /// <summary>
    /// Comment-related domain errors
    /// </summary>
    public static class Comment
    {
        public static Error NotFound(int commentId) =>
            Error.NotFound("Comment.NotFound", $"Comment with ID '{commentId}' was not found.");

        public static readonly Error InvalidContent =
            Error.Validation("Comment.InvalidContent", "Comment content cannot be empty.");

        public static readonly Error ContentTooLong =
            Error.Validation("Comment.ContentTooLong", "Comment content cannot exceed 2000 characters.");

        public static readonly Error InvalidAuthorId =
            Error.Validation("Comment.InvalidAuthorId", "Invalid author ID.");

        public static readonly Error InvalidTicketId =
            Error.Validation("Comment.InvalidTicketId", "Invalid ticket ID.");
    }

    /// <summary>
    /// Tag-related domain errors
    /// </summary>
    public static class Tag
    {
        public static Error NotFound(int tagId) =>
            Error.NotFound("Tag.NotFound", $"Tag with ID '{tagId}' was not found.");

        public static Error NameAlreadyExists(string name) =>
            Error.Conflict("Tag.NameAlreadyExists", $"Tag with name '{name}' already exists.");

        public static readonly Error InvalidName =
            Error.Validation("Tag.InvalidName", "Tag name cannot be empty or exceed 50 characters.");

        public static readonly Error TagAlreadyAdded =
            Error.Validation("Tag.AlreadyAdded", "This tag is already added to the ticket.");
    }

    /// <summary>
    /// Authentication-related domain errors
    /// </summary>
    public static class Authentication
    {
        public static readonly Error InvalidToken =
            Error.Unauthorized("Auth.InvalidToken", "Invalid or expired token.");

        public static readonly Error RefreshTokenExpired =
            Error.Unauthorized("Auth.RefreshTokenExpired", "Refresh token has expired.");

        public static readonly Error RefreshTokenNotFound =
            Error.NotFound("Auth.RefreshTokenNotFound", "Refresh token not found.");

        public static readonly Error TokenRevoked =
            Error.Unauthorized("Auth.TokenRevoked", "Token has been revoked.");
    }

    /// <summary>
    /// Rate limiting domain errors
    /// </summary>
    public static class RateLimit
    {
        public static Error DailyTicketLimitExceeded(int limit) =>
            Error.RateLimitExceeded("RateLimit.DailyTicketLimitExceeded", 
                $"Daily ticket limit of {limit} exceeded. Please try again tomorrow.");

        public static Error DailyCriticalTicketLimitExceeded(int limit) =>
            Error.RateLimitExceeded("RateLimit.DailyCriticalTicketLimitExceeded", 
                $"Daily critical ticket limit of {limit} exceeded.");

        public static Error TooManyRecentTickets(int hours) =>
            Error.RateLimitExceeded("RateLimit.TooManyRecentTickets", 
                $"Too many tickets created in the last {hours} hours. Please wait before creating another ticket.");
    }

    /// <summary>
    /// General domain errors
    /// </summary>
    public static class General
    {
        public static readonly Error UnexpectedError =
            Error.Internal("General.UnexpectedError", "An unexpected error occurred.");

        public static readonly Error ConcurrencyConflict =
            Error.Conflict("General.ConcurrencyConflict", "The resource was modified by another user. Please refresh and try again.");

        public static Error ValidationFailed(string details) =>
            Error.Validation("General.ValidationFailed", details);
    }
}
