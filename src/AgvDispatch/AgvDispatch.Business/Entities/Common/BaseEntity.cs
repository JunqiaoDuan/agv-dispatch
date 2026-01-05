using System.ComponentModel.DataAnnotations;
using AgvDispatch.Shared.Repository;

namespace AgvDispatch.Business.Entities.Common;

public abstract class BaseEntity : IAggregateRoot
{
    [Key]
    public Guid Id { get; set; }

    public bool IsValid { get; set; }
    public string? ReasonOfInvalid { get; set; }

    public Guid? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTimeOffset? CreationDate { get; set; }

    public Guid? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }

    public void OnCreate(Guid? userId = null, string? userName = null)
    {
        Id = Guid.NewGuid();

        IsValid = true;
        ReasonOfInvalid = null;

        CreatedBy = userId;
        CreatedByName = userName;
        CreationDate = DateTimeOffset.Now;

        ModifiedBy = null;
        ModifiedByName = null;
        ModifiedDate = null;
    }

    public void OnUpdate(Guid? userId = null, string? userName = null)
    {
        ModifiedBy = userId;
        ModifiedByName = userName;
        ModifiedDate = DateTimeOffset.Now;
    }

    public void OnDelete(string reasonOfInvalid, Guid? userId = null, string? userName = null)
    {
        IsValid = false;
        ReasonOfInvalid = reasonOfInvalid;

        ModifiedBy = userId;
        ModifiedByName = userName;
        ModifiedDate = DateTimeOffset.Now;
    }
}
