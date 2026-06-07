using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Occasions.Create;

public record CreateOccasionCommand(string Key, string Label) : ICommand<Guid>;
