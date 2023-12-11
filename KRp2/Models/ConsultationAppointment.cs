using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KR2.Models; 

public class ConsultationAppointment {
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(Owner))]
    public int OwnerId { get; set; }
    [ForeignKey(nameof(Consultation))]
    public int ConsultationId { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    public Owner? Owner { get; set; }
    [Required(ErrorMessage = "Заполните это поле")]
    public Consultation? Consultation { get; set; }
}