using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Fort.Utils.WebSocket;
using Fort.Models;

namespace Fort.Services
{
  public class ConnectionsService
  {
    public ConnectionsService()
    {
      _serviceProviders = new HashSet<IServiceProvider>();
    }

    private HashSet<IServiceProvider> _serviceProviders;

    public void NewConnection(IServiceProvider connectionServiceProvider)
    {
      _serviceProviders.Add(connectionServiceProvider);
    }
    public void Disconnected(IServiceProvider connectionServiceProvider)
    {
      _serviceProviders.Remove(connectionServiceProvider);
    }

    public WsConnection GetUsersConnection(Guid userId)
    {
      return _serviceProviders
        .FirstOrDefault(sp => sp.GetService<JwtUser>()?.UserId == userId)?
        .GetService<WsConnection>();
    }

    public WsConnection[] GetTeamConnections(Guid teamId)
    {
      return _serviceProviders
        .Where(sp => sp.GetService<JwtUser>().TeamId == teamId)
        .Select(sp => sp.GetService<WsConnection>())
        .ToArray();
    }

    public WsConnection[] GetAllConnections()
    {
      return _serviceProviders
        .Select(sp => sp.GetService<WsConnection>())
        .ToArray();
    }
  }
}