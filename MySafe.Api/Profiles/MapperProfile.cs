using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using MySafe.Api.Models;
using MySafe.Data.Entities;

namespace MySafe.Api.Profiles
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<JwtSecurityToken, ApplicationToken>()
                .ForMember(d => d.JwtToken, mo => mo.MapFrom(s => s.RawData))
                .ForMember(d => d.Expires, mo => mo.MapFrom(s => s.ValidTo))
                .ForMember(d => d.Created, mo => mo.MapFrom(s => s.IssuedAt))
                .ForAllOtherMembers(options => options.Ignore())
                ;

            CreateMap<UserRequest, ApplicationUser>()
                .ForMember(d => d.UserName, mo => mo.MapFrom(s => s.Username))
                .ForMember(d => d.Email, mo => mo.MapFrom(s => s.Email))
                .ForAllOtherMembers(options => options.Ignore())
                ;
        }
    }
}
