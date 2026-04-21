using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Occasions.Update;

public record UpdateOccasionCommand(Guid Id, string Key, string Label, int Order) : ICommand;
