using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tavenem.Randomize
{
    /// <summary>
    /// Converts a <see cref="RandomParameters"/> to or from JSON.
    /// </summary>
    public class RandomParametersConverter : JsonConverter<RandomParameters>
    {
        /// <summary>Reads and converts the JSON to type <see cref="RandomParameters"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override RandomParameters Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => RandomParameters.ParseExact(reader.GetString(), "r", CultureInfo.InvariantCulture);

        /// <summary>Writes a specified value as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, RandomParameters value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("r"));
    }
}
