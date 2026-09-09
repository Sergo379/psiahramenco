# 🎫 Exam Ticket Generator

> 🧑‍💻 Лабораторная работа №1  
> 🎓 Генератор экзаменационных билетов  
> 💻 C# / .NET  
> 📊 Excel Journal  
> 🧪 Unit Tests  
> 🚀 Git + GitHub

---

<p align="center">

![C#](https://img.shields.io/badge/C%23-.NET-blueviolet?style=for-the-badge&logo=csharp)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Tests](https://img.shields.io/badge/Tests-3%20passed-success?style=for-the-badge)
![Git](https://img.shields.io/badge/Git-version%20controlled-orange?style=for-the-badge&logo=git)
![GitHub](https://img.shields.io/badge/GitHub-repository-black?style=for-the-badge&logo=github)

</p>

---

## 🧠 Что это вообще такое?

Это консольное приложение для генерации экзаменационных билетов.

Пользователь вводит:

- 👤 фамилию;
- 👤 имя.

Программа:

1. 🎲 случайным образом выбирает экзаменационный билет;
2. 🖥️ показывает его в консоли;
3. 📊 сохраняет результат в `journal.xlsx`;
4. 💾 сразу записывает данные на диск;
5. 🔁 позволяет продолжать работу со следующим студентом;
6. 🛑 завершает работу при нажатии `ESC`.

Короче:

```text
Студент → ввод имени → 🎲 билет → 📊 Excel → следующий студент
                                      ↓
                                   💾 SAVE
