namespace UserAccess.Api.Services;

public interface IUserStore
{
    bool TryCreate(string email, string password, string displayName, out UserAccount user);

    bool TryValidateCredentials(string email, string password, out UserAccount user);

    bool TryGetById(Guid userId, out UserAccount user);

    bool TryGetByEmail(string email, out UserAccount user);
}
