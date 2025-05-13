/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyApp.Models
{
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва завдання є обов'язковою.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Термін виконання є обов'язковим.")]
        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; } = false; 
        
        [Required]
        public string UserId { get; set; }  

        [ForeignKey("UserId")]
        public string Author { get; set; }  
    }
}*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyApp.Models
{
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва завдання є обов'язковою.")]
        [Display(Name = "Назва")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Термін виконання є обов'язковим.")]
        [Display(Name = "Дедлайн")]
        public DateTime Deadline { get; set; }

        [Display(Name = "Завершено")]
        public bool IsCompleted { get; set; } = false;

        [Required]
        [Display(Name = "Ідентифікатор користувача")]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [Display(Name = "Автор")]
        public string Author { get; set; }
    }
}
