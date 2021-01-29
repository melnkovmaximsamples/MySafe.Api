using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySafe.Api.Models;

namespace MySafe.Api.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MySafeDbContext _context;

        public UsersController(MySafeDbContext context)
        {
            _context = context;
        } 
    }
}
