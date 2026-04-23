using FluentValidation;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must be 1000 characters or fewer.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority value is invalid.");
    }
}
