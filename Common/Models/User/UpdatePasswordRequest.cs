namespace NV.CT.Models.User;

public class UpdatePasswordRequest
{
	public string UserEntityId { get; set; }

	public string HashedNewPassword { get; set; }

	public UpdatePasswordRequest(string userEntityId,string hashedNewPassword)
	{
		UserEntityId=userEntityId;
		HashedNewPassword=hashedNewPassword;
	}
}