using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

public class FileValidationAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions;
    private readonly int _maxFileSizeInMB;

    public FileValidationAttribute(string[] allowedExtensions, int maxFileSizeInMB)
    {
        _allowedExtensions = allowedExtensions;
        _maxFileSizeInMB = maxFileSizeInMB;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
            }

            if (file.Length > _maxFileSizeInMB * 1024 * 1024)
            {
                return new ValidationResult($"File size must not exceed {_maxFileSizeInMB} MB.");
            }
        }

        return ValidationResult.Success;
    }
}
