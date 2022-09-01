using System.Collections;
using System.Collections.Immutable;

namespace WerewolfEngine;

public class TagSet : IEquatable<TagSet>, IEnumerable<Tag> //, IReadOnlySet<Tag>, IEnumerable<Tag>
{
    private readonly IImmutableSet<Tag> _set;
    
    public int Count => _set.Count;

    public TagSet(IImmutableSet<Tag> set) => _set = set;
    public TagSet(IEnumerable<Tag> tags) : this(tags.ToImmutableHashSet()) {}
    public TagSet(params Tag[] tags) : this((IEnumerable<Tag>)tags) {}

    public bool IsSubsetOf(TagSet tagSet) => _set.IsSubsetOf(tagSet._set);
    public bool IsSupersetOf(TagSet tagSet) => _set.IsSupersetOf(tagSet._set);
    public bool Equals(TagSet? tagSet) => tagSet is not null && _set.SetEquals(tagSet._set);
    public bool Contains(Tag value) => _set.Contains(value);

    public bool TryGetValue(Tag equalValue, out Tag actualValue) => _set.TryGetValue(equalValue, out actualValue);
    
    public TagSet Add(Tag value) => new(_set.Add(value));
    public TagSet Except(IEnumerable<Tag> other) => new(_set.Except(other));
    public TagSet Intersect(IEnumerable<Tag> other) => new(_set.Intersect(other));
    public TagSet Remove(Tag value) => new(_set.Remove(value));
    public TagSet Union(IEnumerable<Tag> other) => new(_set.Union(other));

    // just the identifier has to match, then they are considered equal
    // TODO not tested yet
    public TagSet Replace(Tag updatedTag) => Remove(updatedTag).Add(updatedTag);

    // public TagSet Clone() => new(_set.ToImmutableHashSet());

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
}