using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Delete;

public record DeleteFulfilledBadgeDefinitionCommand(int Id) : ICommand;
