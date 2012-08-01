using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace IN.Chat.Web.Models.Account
{
    public class CreateModel
    {
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        [Required]
        public string EmailAddress { get; set; }

        [Display(Name = "Username")]
        [Required]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }

        [Display(Name = "Pin")]
        [Required]
        public string Pin { get; set; }

        [Display(Name = "Question One")]
        [Required]
        public string QuestionOneId { get; set; }

        [Display(Name = "Answer One")]
        [Required]
        public string QuestionOneAnswer { get; set; }

        [Display(Name = "Question Two")]
        [Required]
        public string QuestionTwoId { get; set; }

        [Display(Name = "Answer Two")]
        [Required]
        public string QuestionTwoAnswer { get; set; }

        [Display(Name = "Question Three")]
        [Required]
        public string QuestionThreeId { get; set; }

        [Display(Name = "Answer Three")]
        [Required]
        public string QuestionThreeAnswer { get; set; }

        public IEnumerable<SelectListItem> SecurityQuestions { get; set; }
    }
}