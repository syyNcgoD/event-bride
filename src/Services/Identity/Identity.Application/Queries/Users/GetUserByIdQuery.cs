using AutoMapper;
using Identity.Application.Common.Models;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Queries.Users;

public record GetUserByIdQuery(string UserId) : IRequest<ApiResponse<UserResponse>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(UserManager<User> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return ApiResponse<UserResponse>.Fail("کاربر یافت نشد");
        }

        var response = _mapper.Map<UserResponse>(user);
        return ApiResponse<UserResponse>.Ok(response);
    }
}
