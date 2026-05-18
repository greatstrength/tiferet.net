using System.Collections.Concurrent;
using Tiferet.Events;

namespace Tiferet.Examples.WebApi.Events;

// *** Shared in-memory store (singleton via DI config)

/// <summary>
/// Simple in-memory task store used by all task events.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public class TaskStore
{
    private readonly ConcurrentDictionary<string, TaskItem> _items = new();
    private int _nextId;

    public TaskItem? Get(string id) => _items.GetValueOrDefault(id);
    public IReadOnlyList<TaskItem> List() => _items.Values.ToList();
    public void Set(string id, TaskItem item) => _items[id] = item;
    public bool Remove(string id) => _items.TryRemove(id, out _);
    public string NextId() => Interlocked.Increment(ref _nextId).ToString();
}

/// <summary>Simple task record.</summary>
public record TaskItem(string Id, string Title, string Status, string? Description = null);

// *** Parameter records

public sealed record ValidateTaskParams(string Title, string? Description = null);
public sealed record TaskIdParams(string Id);
public sealed record UpdateTaskParams(string Id, string? Title = null, string? Status = null, string? Description = null);

// *** Events

/// <summary>
/// Validate task input (step 1 of create workflow).
/// Returns a validated TaskItem with a generated ID.
/// </summary>
public class ValidateTask : DomainEvent<ValidateTaskParams, TaskItem>
{
    private readonly TaskStore _store;
    public ValidateTask(TaskStore store) => _store = store;

    public override TaskItem Execute(ValidateTaskParams p)
    {
        Verify(
            !string.IsNullOrWhiteSpace(p.Title),
            "MISSING_TITLE",
            "Task title is required.");

        var id = _store.NextId();
        return new TaskItem(id, p.Title.Trim(), "pending", p.Description?.Trim());
    }
}

/// <summary>
/// Persist a task (step 2 of create workflow).
/// Reads the validated task from the pipeline data via DataKey.
/// </summary>
public class PersistTask : AsyncDomainEvent<TaskIdParams, TaskItem>
{
    private readonly TaskStore _store;
    public PersistTask(TaskStore store) => _store = store;

    public override async Task<TaskItem> ExecuteAsync(TaskIdParams p)
    {
        // Simulate async I/O (e.g., database write).
        await Task.Delay(1);

        // The validated task is passed via the request data pipeline.
        // For this event, we read it from the store after creation.
        var task = _store.Get(p.Id);
        Verify(task is not null, "TASK_NOT_FOUND", $"Task {p.Id} not found.");
        return task!;
    }
}

/// <summary>
/// Save a validated task to the store.
/// Designed as step 2 in the create pipeline — reads the validated_task from data.
/// </summary>
public class SaveTask : DomainEvent<ValidateTaskParams, TaskItem>
{
    private readonly TaskStore _store;
    public SaveTask(TaskStore store) => _store = store;

    public override TaskItem Execute(ValidateTaskParams p)
    {
        var id = _store.NextId();
        var task = new TaskItem(id, p.Title.Trim(), "pending", p.Description?.Trim());
        _store.Set(id, task);
        return task;
    }
}

/// <summary>List all tasks.</summary>
public class ListTasks : DomainEvent<TaskIdParams, IReadOnlyList<TaskItem>>
{
    private readonly TaskStore _store;
    public ListTasks(TaskStore store) => _store = store;

    public override IReadOnlyList<TaskItem> Execute(TaskIdParams p) => _store.List();
}

/// <summary>Get a single task by ID.</summary>
public class GetTask : DomainEvent<TaskIdParams, TaskItem>
{
    private readonly TaskStore _store;
    public GetTask(TaskStore store) => _store = store;

    public override TaskItem Execute(TaskIdParams p)
    {
        var task = _store.Get(p.Id);
        Verify(task is not null, "TASK_NOT_FOUND", $"Task {p.Id} not found.");
        return task!;
    }
}

/// <summary>Update a task's fields.</summary>
public class UpdateTask : DomainEvent<UpdateTaskParams, TaskItem>
{
    private readonly TaskStore _store;
    public UpdateTask(TaskStore store) => _store = store;

    public override TaskItem Execute(UpdateTaskParams p)
    {
        var existing = _store.Get(p.Id);
        Verify(existing is not null, "TASK_NOT_FOUND", $"Task {p.Id} not found.");

        var updated = existing! with
        {
            Title = p.Title?.Trim() ?? existing.Title,
            Status = p.Status?.Trim() ?? existing.Status,
            Description = p.Description?.Trim() ?? existing.Description
        };

        _store.Set(p.Id, updated);
        return updated;
    }
}

/// <summary>Delete a task by ID.</summary>
public class DeleteTask : DomainEvent<TaskIdParams, string>
{
    private readonly TaskStore _store;
    public DeleteTask(TaskStore store) => _store = store;

    public override string Execute(TaskIdParams p)
    {
        var exists = _store.Get(p.Id);
        Verify(exists is not null, "TASK_NOT_FOUND", $"Task {p.Id} not found.");
        _store.Remove(p.Id);
        return p.Id;
    }
}
