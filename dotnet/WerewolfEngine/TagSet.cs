using System.Collections;
using System.Collections.Immutable;

namespace WerewolfEngine;

public class TagSet : IEquatable<TagSet>, IEnumerable<Tag> //, IReadOnlySet<Tag>, IEnumerable<Tag>
{
    private readonly IImmutableSet<Tag> _set;
    
    public int Count => _set.Count;

    public TagSet(params Tag[] tags) : this((IEnumerable<Tag>)tags) {}
    private TagSet(IImmutableSet<Tag> set) => _set = set;
    private TagSet(IEnumerable<Tag> tags) : this(tags.ToImmutableHashSet()) {}

    public bool TryGetValue(Tag equalValue, out Tag actualValue) => _set.TryGetValue(equalValue, out actualValue);
    
    public TagSet Add(Tag value) => new(_set.Add(value));
    public TagSet Except(IEnumerable<Tag> other) => new(_set.Except(other));
    public TagSet Intersect(IEnumerable<Tag> other) => new(_set.Intersect(other));
    public TagSet Remove(Tag value) => new(_set.Remove(value));
    public TagSet Union(IEnumerable<Tag> other) => new(_set.Union(other));

    // just the identifier has to match, then they are considered equal, so it first removes the old tag then adds the new one
    public TagSet AddOrReplace(Tag updatedTag) => Remove(updatedTag).Add(updatedTag);
    // may return the same instance if the tag doesn't exist!
    public TagSet ReplaceIfExists(Tag updatedTag) => TryGetValue(updatedTag, out _) ? AddOrReplace(updatedTag) : this;

    public IEnumerator<Tag> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _set).GetEnumerator();

    /*
    public bool Contains(Tag item) => _set.Contains(item);
    public bool IsProperSubsetOf(IEnumerable<Tag> other) => _set.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<Tag> other) => _set.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<Tag> other) => _set.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<Tag> other) => _set.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<Tag> other) => _set.Overlaps(other);
    public bool SetEquals(IEnumerable<Tag> other) => _set.SetEquals(other);
    */

    public override string ToString() => "{" + string.Join(", ", _set) + "}";

    public bool IsSubsetOf(TagSet tagSet) => _set.IsSubsetOf(tagSet._set);
    public bool IsSupersetOf(TagSet tagSet) => _set.IsSupersetOf(tagSet._set);
    public bool Contains(Tag value) => _set.Contains(value);
    public bool Equals(TagSet? tagSet) => tagSet is not null && _set.SetEquals(tagSet._set);
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TagSet) obj);
    }

    public override int GetHashCode() => _set.GetHashCode();
    public static bool operator ==(TagSet? left, TagSet? right) => Equals(left, right);
    public static bool operator !=(TagSet? left, TagSet? right) => !Equals(left, right);
}