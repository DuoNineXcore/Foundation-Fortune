using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FoundationFortune.API.YamlConverters;

public sealed class VectorsConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4);

    public object ReadYaml(IParser parser, Type type)
    {
        if (!parser.TryConsume<MappingStart>(out _))
            throw new InvalidDataException($"Cannot deserialize object of type {type.FullName}.");

        List<float> coordinates = new List<float>();

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (!parser.TryConsume<Scalar>(out var key) || !parser.TryConsume<Scalar>(out var value) || !float.TryParse(value.Value, NumberStyles.Float, CultureInfo.GetCultureInfo("en-US"), out float coordinate))
            {
                throw new InvalidDataException($"Invalid float value.");
            }

            coordinates.Add(coordinate);
        }

        object vector;
        
        if (type == typeof(Vector2) && coordinates.Count >= 2) vector = new Vector2(coordinates[0], coordinates[1]);
        else if (type == typeof(Vector3) && coordinates.Count >= 3) vector = new Vector3(coordinates[0], coordinates[1], coordinates[2]);
        else if (type == typeof(Vector4) && coordinates.Count >= 4) vector = new Vector4(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
        else throw new InvalidDataException($"Invalid number of coordinates for {type.FullName}.");

        return vector;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        Dictionary<string, float> coordinates = new Dictionary<string, float>();

        switch (value)
        {
            case Vector2 vector2:
                coordinates["x"] = vector2.x;
                coordinates["y"] = vector2.y;
                break;
            case Vector3 vector3:
                coordinates["x"] = vector3.x;
                coordinates["y"] = vector3.y;
                coordinates["z"] = vector3.z;
                break;
            case Vector4 vector4:
                coordinates["x"] = vector4.x;
                coordinates["y"] = vector4.y;
                coordinates["z"] = vector4.z;
                coordinates["w"] = vector4.w;
                break;
            default:
                throw new ArgumentException($"Unsupported vector type: {type.FullName}");
        }

        emitter.Emit(new MappingStart());

        foreach (var coordinate in coordinates)
        {
            emitter.Emit(new Scalar(coordinate.Key));
            emitter.Emit(new Scalar(coordinate.Value.ToString(CultureInfo.GetCultureInfo("en-US"))));
        }

        emitter.Emit(new MappingEnd());
    }
}

