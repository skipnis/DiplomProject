using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Occasions.Delete;

public record DeleteOccasionCommand(Guid Id) : ICommand;
