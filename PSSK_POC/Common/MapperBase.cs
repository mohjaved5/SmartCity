using PSSK_POC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Common
{
    public class MapperBase : AutoMapper.Profile
    {
        /// <summary>
        /// Auto Mapper Constructor
        /// </summary>

        public MapperBase()
        {

            CreateMap<PersonRequest, PersonResponse>();
        }

    }
}
