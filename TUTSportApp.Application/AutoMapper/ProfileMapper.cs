using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using TUTSportApp.Application.Features.Auth.Commands;
using TUTSportApp.Domain.Models;

namespace TUTSportApp.Application.AutoMapper
{
    public class ProfileMapper : Profile
    {
        public ProfileMapper()
        {
            CreateMap<LoginModel, LoginCommand>()
          .ReverseMap();
        }
    }
}
