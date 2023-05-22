using Microsoft.AspNetCore.SignalR;
namespace RecomField.Hubs;

public class MainHub : Hub
{
    public async void JoinGroup(string groupName) => await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
}
