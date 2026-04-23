using FluentValidation;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must be 1000 characters or fewer.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority value is invalid.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Status!.Value)
            .IsInEnum().WithMessage("Status value is invalid.")
            .When(x => x.Status.HasValue);
    }
}
