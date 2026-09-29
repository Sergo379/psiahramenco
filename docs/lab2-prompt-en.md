# Lab 2: implementation specification

Extend the existing C# examination ticket generator into a Windows WPF desktop application. Keep Excel and Word access independent from the UI and cover the domain logic with xUnit tests.

Input:
- students.xlsx: one worksheet per group, worksheet name is the group identifier. Row 1 contains headers; column A is surname, column B is first name. Read complete student rows starting at row 2; trim surrounding whitespace. Preserve empty groups.
- tickets.docx: headings “Билет N”, followed by numbered question paragraphs. Support literal “1. question” and Word automatic numbering. Skip malformed, incomplete and duplicate tickets, logging the reason. Show exactly three questions from each valid ticket. Choose among actual valid ticket identifiers so skipped tickets never produce invalid draws.

UI:
- Use a loft-inspired visual design: graphite and olive surfaces, warm copper accents, serif display headings, rounded controls and readable contrast.
- Populate the group list on startup. Populate students only after choosing a group. Enable generation only when both selections exist and valid tickets are loaded.
- Save the assignment before opening a modal result window. Display the ticket number, student name and three questions. Escape closes only the result window. Return to selecting the next student.
- Provide source reload, data-folder and journal buttons. Missing or damaged input files should display an actionable status, not terminate the process.

Persistence:
- Create results.xlsx at startup if missing. Headers: Группа, Фамилия, Имя, Номер билета, Дата и время, Повтор.
- Student identity is the exact group + surname + first-name tuple. On repeated selection use the ticket number from the first chronological journal row. Append every issuance, including repeats; mark them “да” or “нет”. Never replace earlier rows.
- Save immediately, use a serialized temporary workbook and replacement rather than truncating the original. Validate existing journal headers.
- Put lookup and saving within the retry boundary. If Excel blocks the journal, offer retry/cancel and retain the running application. If a historical ticket is missing from Word, explain the issue without assigning a different number.

Delivery:
- Supply a self-contained Windows x64 single-file executable requiring no installed .NET or Office.
- Package demonstrative input files without silently overwriting user files on ordinary startup.
- Test Word parsing, malformed headings, automatic numbering, sheet-based student lookup, empty groups, repeat identity, original-history preservation, locked journal retry, and missing historical tickets.
- Verify the actual WPF controls and complete generation flow. Include a README and a reproducible packaging script.
