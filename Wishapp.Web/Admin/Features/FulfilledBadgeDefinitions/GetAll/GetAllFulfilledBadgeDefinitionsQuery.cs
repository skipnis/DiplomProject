using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.GetAll;

public record GetAllFulfilledBadgeDefinitionsQuery : IQuery<List<FulfilledBadgeDefinitionDto>>;
