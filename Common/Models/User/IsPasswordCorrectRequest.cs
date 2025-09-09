namespace NV.CT.Models.User;

public class IsPasswordCorrectRequest
{
	public string OldPassword { get; set; } 
	public string InputPassword { get; set; }


	public IsPasswordCorrectRequest(string oldPassword,string inputPassword)
	{
		OldPassword = oldPassword;
		InputPassword = inputPassword;
	}
}