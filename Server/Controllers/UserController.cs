using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profile.Server.Data;
using Profile.Shared.Models;

namespace Profile.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ProfileContext _context;
        private readonly UserManager<ProfileUser> _userManager;
        private readonly SignInManager<ProfileUser> _signInManager;

        public UserController(ProfileContext context, UserManager<ProfileUser> userManager, SignInManager<ProfileUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string ImageUrl { get; set; }
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<ProfileUser>> GetUser()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        [HttpGet("u/{username}/phone")]
        public async Task<ActionResult<string>> GetUserPhoneNumber(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user.PhoneNumber;
        }

        public class UserModel {
            public string UserId { get; set; }
            public ProfileUserType ProfileUserType { get; set; }
            public bool HasPhoneNumber { get; set; }
            public string ImageUrl { get; set; }
        }
        public UserModel Model = new UserModel();

        [HttpGet("u/{username}")]
        public async Task<ActionResult<UserModel>> GetUserId(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            Model.UserId = user.Id;
            Model.ProfileUserType = user.ProfileUserType;
            Model.ImageUrl = user.ImageUrl;
            if(user.PhoneNumber != null) {
                Model.HasPhoneNumber = true;
            }
            return Model;
        }


        // GET: api/User/loggedin
        [HttpGet("loggedin")]
        public async Task<ActionResult<ProfileUser>> GetLoggedInUser()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }


        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileUser>> GetProfileUser(string id)
        {
            var profileUser = await _context.Users.FindAsync(id);

            if (profileUser == null)
            {
                return NotFound();
            }

            return profileUser;
        }


        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfileUser(string id, ProfileUser profileUser)
        {
            if (id != profileUser.Id)
            {
                return BadRequest();
            }

            //_context.Entry(profileUser).State = EntityState.Modified;

            try
            {

                var user = profileUser;
                var updateUser = await _userManager.UpdateAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //POST: api/User
        //To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/image")]
        public async Task<ActionResult> UpdateProfilePicture(string id, ProfileUser profileUser)
        {
            var user = await _userManager.FindByIdAsync(profileUser.Id);

            if(user.ImageUrl != profileUser.ImageUrl) {
                try
                {
                    user.ImageUrl = profileUser.ImageUrl;
                    IdentityResult result = await _userManager.UpdateAsync(user);
                }
                catch (DbUpdateException)
                {
                    if (ProfileUserExists(id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return NoContent();
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProfileUser>> PostProfileUser(ProfileUser profileUser)
        {
            _context.Users.Add(profileUser);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfileUserExists(profileUser.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfileUser", new { id = profileUser.Id }, profileUser);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfileUser(string id)
        {
            var profileUser = await _context.Users.FindAsync(id);
            if (profileUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(profileUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfileUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

    }
}
