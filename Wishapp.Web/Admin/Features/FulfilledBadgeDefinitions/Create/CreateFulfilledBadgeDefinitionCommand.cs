using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Create;

public record CreateFulfilledBadgeDefinitionCommand(
    string Emoji,
    string Slug,
    string Label,
    string Description) : ICommand<int>;
