-> start

=== start ===
#side:left
Привет! Я лесной стражник.

+ [Спросить имя] -> ask_name
+ [Спросить о лесе] -> ask_forest

=== ask_name ===
#side:left
Меня зовут Элран.
А как зовут тебя?

+ [Назвать свое имя] -> tell_name
+ [Промолчать] -> silent

=== tell_name ===
#side:right
Приятно познакомиться!
-> start

=== silent ===
#side:right
Как хочешь.
-> start

=== ask_forest ===
#side:left
В этом лесу водятся волки.
Будь осторожен!
-> start
