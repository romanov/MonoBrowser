module DataValidation

/// <summary>
/// Convert a potentially null value to an option.
///
/// This is different from <see cref="FSharp.Core.Option.ofObj">Option.ofObj</see> where it doesn't require the value to be constrained to null.
/// This is beneficial where third party APIs may generate a record type using reflection and it can be null.
/// See <a href="https://latkin.org/blog/2015/05/18/null-checking-considerations-in-f-its-harder-than-you-think/">Null-checking considerations in F#</a> for more details.
///
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/ofnull</href>
/// </summary>
/// <param name="value">The potentially null value</param>
/// <returns>An option</returns>
/// <seealso cref="FSharp.Core.Option.ofObj"/>
let ofNull (value: 'nullableValue) : 'nullableValue option =
    if System.Object.ReferenceEquals(value, null) then
        None
    else
        Some value