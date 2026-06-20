using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InstructorPlus.Models;
using InstructorPlus.Models.Enums;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace InstructorPlus.Repositories;

public class DocumentRepository : BaseRepository<Document>, IDisposable
{
    private static List<Document> _documents = new();

    public DocumentRepository(IOptions<DatabaseSettings> options) : base(options)
    {
        OpenConnection();
        LoadDocuments();
    }

    private void LoadDocuments()
    {
        _documents.Clear();

        using var cmd = new MySqlCommand(
            @"SELECT d.id, d.student_id, d.doc_type, d.file_path,
                     d.expiry_date, d.is_valid, s.full_name AS student_name
              FROM documents d
              JOIN students s ON d.student_id = s.id
              ORDER BY d.id", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _documents.Add(new Document
            {
                Id = reader.GetInt32("id"),
                StudentId = reader.GetInt32("student_id"),
                DocType = Enum.Parse<DocType>(reader.GetString("doc_type")),
                FilePath = reader.GetString("file_path"),
                ExpiryDate = reader.GetDateTime("expiry_date"),
                IsValid = reader.GetBoolean("is_valid"),
                StudentName = reader.GetString("student_name"),
            });
        }
    }

    public Document? GetById(int id) =>
        _documents.FirstOrDefault(d => d.Id == id);

    public List<Document> GetAll() => _documents;

    public List<Document> GetByStudent(int studentId) =>
        _documents.FindAll(d => d.StudentId == studentId);

    public void Add(Document document)
    {
        using var cmd = new MySqlCommand(
            @"INSERT INTO documents (student_id, doc_type, file_path, expiry_date, is_valid)
              VALUES (@sid, @type, @path, @expiry, @valid)", connection);

        cmd.Parameters.AddWithValue("@sid", document.StudentId);
        cmd.Parameters.AddWithValue("@type", document.DocType.ToString());
        cmd.Parameters.AddWithValue("@path", document.FilePath);
        cmd.Parameters.AddWithValue("@expiry", document.ExpiryDate);
        cmd.Parameters.AddWithValue("@valid", document.IsValid);
        cmd.ExecuteNonQuery();

        LoadDocuments();
    }

    public void Update(Document document)
    {
        using var cmd = new MySqlCommand(
            @"UPDATE documents SET doc_type = @type, file_path = @path,
                                   expiry_date = @expiry, is_valid = @valid
              WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", document.Id);
        cmd.Parameters.AddWithValue("@type", document.DocType.ToString());
        cmd.Parameters.AddWithValue("@path", document.FilePath);
        cmd.Parameters.AddWithValue("@expiry", document.ExpiryDate);
        cmd.Parameters.AddWithValue("@valid", document.IsValid);
        cmd.ExecuteNonQuery();

        LoadDocuments();
    }

    public void Delete(int id)
    {
        using var cmd = new MySqlCommand("DELETE FROM documents WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        LoadDocuments();
    }

    public new void Dispose()
    {
        CloseConnection();
        base.Dispose();
    }
}