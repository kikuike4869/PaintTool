// PaintTool/Models/Album.cs (修正後)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging; // ★ ImageFormatのために追加
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PaintTool
{
    public class Album
    {
        public string Name { get; set; }
        public Canvas Canvas { get; set; }

        private static readonly string BaseSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PaintToolAlbums");

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            // TypeInfoResolver = new DefaultJsonTypeInfoResolver(), // ← この行を削除またはコメントアウト
            Converters = { new ColorJsonConverter(), new FontJsonConverter() }
        };

        public Album(string name, Canvas canvas)
        {
            Name = name;
            Canvas = canvas;
        }

        // ★★★ 仕様変更に合わせてSaveメソッドを修正 ★★★
        // プレビュー用の画像を受け取るように変更
        public void Save(Image previewImage)
        {
            var albumDir = GetAlbumDirectoryPath(Name);
            if (!Directory.Exists(albumDir))
            {
                Directory.CreateDirectory(albumDir);
            }

            // 1. JSON情報の保存
            var json = JsonSerializer.Serialize(this.Canvas, jsonOptions);
            File.WriteAllText(Path.Combine(albumDir, "album.json"), json);

            // 2. プレビュー画像の保存
            previewImage.Save(Path.Combine(albumDir, "preview.png"), ImageFormat.Png);
        }

        // ★★★ 仕様変更に合わせてLoadメソッドを修正 ★★★
        public static Album Load(string name)
        {
            var albumDir = GetAlbumDirectoryPath(name);
            var jsonPath = Path.Combine(albumDir, "album.json");

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException("Album json file not found.", jsonPath);
            }

            var json = File.ReadAllText(jsonPath);
            var canvas = JsonSerializer.Deserialize<Canvas>(json, jsonOptions);

            if (canvas == null)
            {
                throw new InvalidDataException("Failed to load canvas from album.");
            }

            // 読み込んだ情報からAlbumインスタンスを生成
            var album = new Album(name, canvas);
            return album;
        }

        // ★★★ 仕様変更に合わせてGetAlbumNamesメソッドを修正 ★★★
        // ディレクトリ名の一覧を取得するように変更
        public static string[] GetAlbumNames()
        {
            if (!Directory.Exists(BaseSavePath))
            {
                return Array.Empty<string>();
            }
            return Directory.GetDirectories(BaseSavePath)
                            .Select(Path.GetFileName)
                            .ToArray()!;
        }

        // アルバムのディレクトリパスを取得するヘルパーメソッド
        private static string GetAlbumDirectoryPath(string name)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                name = name.Replace(c, '_');
            }
            return Path.Combine(BaseSavePath, name);
        }

        // プレビュー画像を取得するメソッド
        public static Image? GetPreviewImage(string name)
        {
            var previewPath = Path.Combine(GetAlbumDirectoryPath(name), "preview.png");
            if (!File.Exists(previewPath))
            {
                return null;
            }
            // ファイルをロックしないように、一度メモリに読み込んでから返す
            using (var bmp = new Bitmap(previewPath))
            {
                return new Bitmap(bmp);
            }
        }
    }

    public class FontJsonConverter : JsonConverter<Font>
    {
        public override Font Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            string fontFamily = "Arial";
            float emSize = 12f;
            FontStyle style = FontStyle.Regular;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Font(fontFamily, emSize, style);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "FamilyName":
                            fontFamily = reader.GetString() ?? fontFamily;
                            break;
                        case "EmSize":
                            emSize = reader.GetSingle();
                            break;
                        case "Style":
                            // Enumを文字列として読み取る
                            style = JsonSerializer.Deserialize<FontStyle>(ref reader, options);
                            break;
                    }
                }
            }
            throw new JsonException("Unexpected end of JSON.");
        }

        public override void Write(Utf8JsonWriter writer, Font value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("FamilyName", value.FontFamily.Name);
            writer.WriteNumber("EmSize", value.Size);
            // Enumを文字列として書き出す
            writer.WritePropertyName("Style");
            JsonSerializer.Serialize(writer, value.Style, options);
            writer.WriteEndObject();
        }
    }

    // ★ 追加: Color型をJSONに変換するためのカスタムコンバータ
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            int a = 255, r = 0, g = 0, b = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return Color.FromArgb(a, r, g, b);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString()?.ToLower();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "a": a = reader.GetInt32(); break;
                        case "r": r = reader.GetInt32(); break;
                        case "g": g = reader.GetInt32(); break;
                        case "b": b = reader.GetInt32(); break;
                    }
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("A", value.A);
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }
}