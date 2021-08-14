using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab4.Data;
using Lab4.Models;
using Lab4.Models.ViewModels;

namespace Lab4.Controllers
{
    public class StudentController : Controller
    {
        private readonly SchoolCommunityContext _context;

        public StudentController(SchoolCommunityContext context)
        {
            _context = context;
        }

        // GET: Student
        public async Task<IActionResult> Index(int? ID)
        {
            var viewModel = new CommunityViewModel();
            viewModel.Students = await _context.Students
                .Include(i => i.Membership)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (ID != null)
            {
                ViewData["StudentId"] = ID;
                viewModel.CommunityMemberships = await _context.CommunityMemberships
                .Include(i => i.Community)
                .Include(i => i.Student)
                .AsNoTracking()
                .ToListAsync();
                viewModel.Communities = viewModel.CommunityMemberships.Where(
                    x => x.StudentId == ID).Select(f => f.Community);
            }
            return View(viewModel);
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,EnrollmentDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }


        public async Task<IActionResult> AddMembership(string CommunityId, int StudentId)
        {
                CommunityMembership newMember = new CommunityMembership();
                newMember.StudentId = StudentId;
                newMember.CommunityId = CommunityId;
                _context.Add(newMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemoveMembership(string CommunityId, int StudentId)
        {
            CommunityMembership newMember = new CommunityMembership();
            newMember.StudentId = StudentId;
            newMember.CommunityId = CommunityId;
            _context.CommunityMemberships.Remove(newMember);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Student/EditMembership
        public async Task<IActionResult> EditMembership(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = new MembershipViewModel();
            viewModel.Student = await _context.Students.FindAsync(id);
            IEnumerable<CommunityMembership> temp = await _context.CommunityMemberships
                .Include(i => i.Community)
                .Include(i => i.Student)
                .OrderBy(i => i.CommunityId)
                .AsNoTracking()
                .ToListAsync();
            viewModel.InvolvedCommunities = temp.Where(
                    x => x.StudentId == id).Select(f => f.Community);

            IEnumerable<Community> temp2 = await _context.Communities
                .OrderBy(i => i.Id)
                .ToListAsync();
            viewModel.UninvolvedCommunities = temp2.Except(viewModel.InvolvedCommunities);
            return View(viewModel);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,EnrollmentDate")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
