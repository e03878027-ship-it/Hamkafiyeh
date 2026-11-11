using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hamkafiyeh;

public partial class MainPage : ContentPage
{
    private string userName = "";
    private string userFamily = "";
    private string userPhone = "";
    private string userPassword = "";
    private bool isLoggedIn = false;

    private readonly List<Product> products = new();
    private readonly List<EventItem> events = new();
    private readonly List<CartItem> cart = new();
    private readonly List<TicketItem> tickets = new();

    private readonly HashSet<string> favorites = new();
    private readonly Dictionary<string, HashSet<string>> reservedSeatsByEvent = new();

    private const string Red = "#D20F1D";
    private const string AdminDemoPassword = "2468";

    public MainPage()
    {
        InitializeComponent();

        products.Add(new Product("محصول ویژه همکفیه", "محصول اختصاصی فروشگاه", 150000));
        products.Add(new Product("یادگاری همکفیه", "یادگاری از تجربه‌های همکفیه", 200000));
        products.Add(new Product("محصول اختصاصی", "محصول ویژه علاقه‌مندان", 100000));

        events.Add(new EventItem("اجرای ویژه همکفیه", "تئاتر", "زمان اجرا به‌زودی اعلام می‌شود", 150000));
        events.Add(new EventItem("تجربه سینما ترس", "سینما ترس", "زمان سانس به‌زودی اعلام می‌شود", 120000));
        events.Add(new EventItem("ماجراجویی اتاق فرار", "اتاق فرار", "زمان سانس به‌زودی اعلام می‌شود", 180000));
        events.Add(new EventItem("دورهمی همکفیه", "همایش و رویداد", "زمان برگزاری به‌زودی اعلام می‌شود", 100000));
    }

    private async void HomeClicked(object sender, TappedEventArgs e)
    {
        await DisplayAlert("همکفیه", "شما در صفحه اصلی هستید.", "باشه");
    }

    private async void SearchClicked(object sender, EventArgs e)
    {
        string search = SearchInput.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(search))
        {
            search = await DisplayPromptAsync("جستجو", "نام بخش موردنظر را وارد کن:");
        }

        if (string.IsNullOrWhiteSpace(search))
            return;

        search = search.Trim();

        if (search.Contains("تئاتر"))
            await TheaterPage();
        else if (search.Contains("ترس") || search.Contains("سینما"))
            await HorrorPage();
        else if (search.Contains("فرار"))
            await EscapePage();
        else if (search.Contains("همایش") || search.Contains("رویداد"))
            await EventsPage();
        else if (search.Contains("فروشگاه") || search.Contains("محصول"))
            await ShopPage();
        else if (search.Contains("سبد"))
            await CartPage();
        else if (search.Contains("بلیت"))
            await TicketsPage();
        else
            await DisplayAlert("نتیجه جستجو", "موردی با این نام پیدا نشد.", "باشه");
    }

    private async void SearchTabClicked(object sender, TappedEventArgs e)
    {
        string search = await DisplayPromptAsync("جستجو", "نام بخش موردنظر را وارد کن:");

        if (string.IsNullOrWhiteSpace(search))
            return;

        SearchInput.Text = search;
        await SearchResult(search);
    }

    private async Task SearchResult(string search)
    {
        if (search.Contains("تئاتر"))
            await TheaterPage();
        else if (search.Contains("ترس") || search.Contains("سینما"))
            await HorrorPage();
        else if (search.Contains("فرار"))
            await EscapePage();
        else if (search.Contains("همایش") || search.Contains("رویداد"))
            await EventsPage();
        else if (search.Contains("فروشگاه") || search.Contains("محصول"))
            await ShopPage();
        else if (search.Contains("سبد"))
            await CartPage();
        else if (search.Contains("بلیت"))
            await TicketsPage();
        else
            await DisplayAlert("نتیجه جستجو", "موردی پیدا نشد.", "باشه");
    }

    private async void TicketsClicked(object sender, TappedEventArgs e)
    {
        await TicketsPage();
    }

    private async void AccountClicked(object sender, TappedEventArgs e)
    {
        await ShowAccountPage();
    }

    private async void TheaterClicked(object sender, TappedEventArgs e)
    {
        await CategoryPage("تئاتر", "theater.jpg");
    }

    private async void HorrorClicked(object sender, TappedEventArgs e)
    {
        await CategoryPage("سینما ترس", "horror.jpg");
    }

    private async void EscapeClicked(object sender, TappedEventArgs e)
    {
        await CategoryPage("اتاق فرار", "escape.jpg");
    }

    private async void EventsClicked(object sender, TappedEventArgs e)
    {
        await EventsPage();
    }

    private async void ShopClicked(object sender, TappedEventArgs e)
    {
        await ShopPage();
    }

    private Task TheaterPage()
    {
        return CategoryPage("تئاتر", "theater.jpg");
    }

    private Task HorrorPage()
    {
        return CategoryPage("سینما ترس", "horror.jpg");
    }

    private Task EscapePage()
    {
        return CategoryPage("اتاق فرار", "escape.jpg");
    }

    private async Task CategoryPage(string category, string image)
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading(category));
        layout.Children.Add(new Image
        {
            Source = image,
            HeightRequest = 190,
            Aspect = Aspect.AspectFill
        });

        layout.Children.Add(CreateDescription("برنامه‌های این بخش را مشاهده و برای رزرو اقدام کن."));

        var categoryEvents = events.Where(x => x.Category == category).ToList();

        if (categoryEvents.Count == 0)
        {
            layout.Children.Add(CreateDescription("فعلاً برنامه‌ای در این بخش ثبت نشده است."));
        }
        else
        {
            foreach (var item in categoryEvents)
                layout.Children.Add(CreateEventCard(item));
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage(category, layout));
    }

    private async Task EventsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("همایش‌ها و رویدادها"));
        layout.Children.Add(new Image
        {
            Source = "events.jpg",
            HeightRequest = 190,
            Aspect = Aspect.AspectFill
        });

        layout.Children.Add(CreateDescription("برنامه‌های ویژه همکفیه را دنبال کن."));

        var categoryEvents = events
            .Where(x => x.Category == "همایش و رویداد")
            .ToList();

        if (categoryEvents.Count == 0)
            layout.Children.Add(CreateDescription("هنوز رویدادی ثبت نشده است."));

        foreach (var item in categoryEvents)
            layout.Children.Add(CreateEventCard(item));

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("رویدادها", layout));
    }

    private Border CreateEventCard(EventItem item)
    {
        var content = new VerticalStackLayout
        {
            Padding = 14,
            Spacing = 9
        };

        content.Children.Add(CreateHeading(item.Name));
        content.Children.Add(CreateDescription(item.Category));
        content.Children.Add(CreateDescription(item.Description));

        content.Children.Add(new Label
        {
            Text = $"{item.Price:N0} تومان",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(Red)
        });

        var reserve = CreateButton("انتخاب صندلی و رزرو بلیت", Red);

        reserve.Clicked += async (s, e) =>
        {
            if (!isLoggedIn)
            {
                await DisplayAlert("ورود به حساب", "برای رزرو ابتدا وارد حساب کاربری شو.", "باشه");
                await ShowAccountPage();
                return;
            }

            await SeatSelectionPage(item);
        };

        content.Children.Add(reserve);

        var favorite = CreateButton(
            favorites.Contains(item.Name) ? "حذف از علاقه‌مندی‌ها" : "افزودن به علاقه‌مندی‌ها",
            "#333333");

        favorite.Clicked += async (s, e) =>
        {
            if (favorites.Contains(item.Name))
            {
                favorites.Remove(item.Name);
                favorite.Text = "افزودن به علاقه‌مندی‌ها";
            }
            else
            {
                favorites.Add(item.Name);
                favorite.Text = "حذف از علاقه‌مندی‌ها";
            }

            await DisplayAlert("علاقه‌مندی‌ها", "وضعیت علاقه‌مندی تغییر کرد.", "باشه");
        };

        content.Children.Add(favorite);

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 8,
            Content = content
        };
    }

    private async Task SeatSelectionPage(EventItem item)
    {
        if (!reservedSeatsByEvent.TryGetValue(item.Name, out var reservedSeats))
        {
            reservedSeats = new HashSet<string>();
            reservedSeatsByEvent[item.Name] = reservedSeats;
        }

        var selectedSeats = new HashSet<string>();
        var layout = CreateLayout();

        // اصلاح خطای selectedLabel:
        // ابتدا برچسب ساخته می‌شود و سپس دکمه‌های صندلی ایجاد می‌شوند.
        var selectedLabel = new Label
        {
            Text = "صندلی انتخاب‌شده: هنوز انتخابی انجام نشده",
            TextColor = Color.FromArgb("#222222"),
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        void RefreshSeatSelection()
        {
            selectedLabel.Text = selectedSeats.Count == 0
                ? "صندلی انتخاب‌شده: هنوز انتخابی انجام نشده"
                : "صندلی‌های انتخاب‌شده: " + string.Join("، ", selectedSeats.OrderBy(x => x));
        }

        layout.Children.Add(CreateHeading("انتخاب صندلی"));
        layout.Children.Add(CreateDescription($"برنامه: {item.Name}"));
        layout.Children.Add(CreateDescription("نقشه از دید تماشاگران نمایش داده شده است."));

        var stage = new Border
        {
            BackgroundColor = Color.FromArgb("#333333"),
            StrokeThickness = 0,
            Padding = new Thickness(12),
            Margin = new Thickness(24, 8),
            Content = new Label
            {
                Text = "صحنه نمایش",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Center
            }
        };

        layout.Children.Add(stage);

        var legend = new HorizontalStackLayout
        {
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Center
        };

        legend.Children.Add(CreateSeatLegend("#2E9D62", "آزاد"));
        legend.Children.Add(CreateSeatLegend("#2376D8", "انتخاب‌شده"));
        legend.Children.Add(CreateSeatLegend(Red, "رزروشده"));

        layout.Children.Add(legend);

        var seatMap = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnSpacing = 5,
            RowSpacing = 18,
            Padding = new Thickness(2, 12)
        };

        var leftSide = new VerticalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        leftSide.Children.Add(CreateDescription("ردیف C"));
        leftSide.Children.Add(CreateSeatButton("C1"));
        leftSide.Children.Add(CreateSeatButton("C2"));
        leftSide.Children.Add(CreateSeatButton("C3"));

        seatMap.Add(leftSide, 0, 1);
        Grid.SetRowSpan(leftSide, 3);

        var centerSeats = new VerticalStackLayout
        {
            Spacing = 20,
            HorizontalOptions = LayoutOptions.Center
        };

        centerSeats.Children.Add(new Label
        {
            Text = "ردیف A - VIP",
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        });

        var rowA = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };

        rowA.Children.Add(CreateSeatButton("A1"));
        rowA.Children.Add(CreateSeatButton("A2"));
        centerSeats.Children.Add(rowA);

        centerSeats.Children.Add(new Label
        {
            Text = "ردیف B",
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        });

        var rowB = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center
        };

        rowB.Children.Add(CreateSeatButton("B1"));
        rowB.Children.Add(CreateSeatButton("B2"));
        rowB.Children.Add(CreateSeatButton("B3"));
        centerSeats.Children.Add(rowB);

        seatMap.Add(centerSeats, 1, 1);
        Grid.SetRowSpan(centerSeats, 3);

        var balcony = new VerticalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        balcony.Children.Add(CreateDescription("بالکن"));
        balcony.Children.Add(CreateSeatButton("D1"));

        seatMap.Add(balcony, 2, 1);
        Grid.SetRowSpan(balcony, 3);

        layout.Children.Add(seatMap);

        layout.Children.Add(selectedLabel);

        Button CreateSeatButton(string seatName)
        {
            bool isReserved = reservedSeats.Contains(seatName);

            var button = new Button
            {
                Text = seatName,
                WidthRequest = 58,
                HeightRequest = 48,
                Padding = 2,
                FontSize = 13,
                CornerRadius = 10,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb(isReserved ? Red : "#2E9D62"),
                IsEnabled = !isReserved
            };

            button.Clicked += (sender, args) =>
            {
                if (selectedSeats.Contains(seatName))
                {
                    selectedSeats.Remove(seatName);
                    button.BackgroundColor = Color.FromArgb("#2E9D62");
                }
                else
                {
                    selectedSeats.Add(seatName);
                    button.BackgroundColor = Color.FromArgb("#2376D8");
                }

                RefreshSeatSelection();
            };

            return button;
        }

        var confirm = CreateButton("تأیید صندلی‌ها و ثبت رزرو آزمایشی", Red);

        confirm.Clicked += async (sender, args) =>
        {
            if (selectedSeats.Count == 0)
            {
                await DisplayAlert("انتخاب صندلی", "ابتدا حداقل یک صندلی انتخاب کن.", "باشه");
                return;
            }

            foreach (var seat in selectedSeats)
            {
                if (reservedSeats.Contains(seat))
                {
                    await DisplayAlert("صندلی ناموجود", $"صندلی {seat} قبلاً رزرو شده است.", "باشه");
                    return;
                }
            }

            string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            foreach (var seat in selectedSeats)
            {
                reservedSeats.Add(seat);
                tickets.Add(new TicketItem(item.Name, seat, date, item.Price));
            }

            string seatsText = string.Join("، ", selectedSeats.OrderBy(x => x));

            await DisplayAlert(
                "رزرو آزمایشی",
                $"صندلی‌های {seatsText} برای برنامه «{item.Name}» ثبت شدند.\nاین رزرو فقط در حافظه همین اجرای برنامه نگهداری می‌شود.",
                "باشه");

            await Navigation.PopAsync();
        };

        layout.Children.Add(confirm);
        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("انتخاب صندلی", layout));
    }

    private View CreateSeatLegend(string color, string text)
    {
        return new HorizontalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new BoxView
                {
                    Color = Color.FromArgb(color),
                    WidthRequest = 13,
                    HeightRequest = 13,
                    CornerRadius = 3,
                    VerticalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = text,
                    FontSize = 12,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }    private async Task ShopPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("فروشگاه همکفیه"));

        layout.Children.Add(new Image
        {
            Source = "shop.jpg",
            HeightRequest = 180,
            Aspect = Aspect.AspectFill
        });

        layout.Children.Add(CreateDescription("محصولات فروشگاه را مشاهده کن."));

        foreach (var product in products)
            layout.Children.Add(CreateProductCard(product));

        var cartButton = CreateButton($"مشاهده سبد خرید ({cart.Count})", "#222222");
        cartButton.Clicked += async (s, e) => await CartPage();

        layout.Children.Add(cartButton);
        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("فروشگاه", layout));
    }

    private Border CreateProductCard(Product product)
    {
        var content = new VerticalStackLayout
        {
            Padding = 14,
            Spacing = 9
        };

        content.Children.Add(CreateHeading(product.Name));
        content.Children.Add(CreateDescription(product.Description));

        content.Children.Add(new Label
        {
            Text = $"{product.Price:N0} تومان",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(Red)
        });

        var addButton = CreateButton("افزودن به سبد خرید", Red);

        addButton.Clicked += async (s, e) =>
        {
            cart.Add(new CartItem(product.Name, product.Price));
            await DisplayAlert("سبد خرید", "محصول به سبد خرید اضافه شد.", "باشه");
        };

        content.Children.Add(addButton);

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 8,
            Content = content
        };
    }

    private async Task CartPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("سبد خرید"));

        if (cart.Count == 0)
        {
            layout.Children.Add(CreateDescription("سبد خرید شما خالی است."));
        }
        else
        {
            decimal total = 0;

            for (int i = 0; i < cart.Count; i++)
            {
                var item = cart[i];
                total += item.Price;

                var row = new HorizontalStackLayout
                {
                    Spacing = 8,
                    VerticalOptions = LayoutOptions.Center
                };

                row.Children.Add(new Label
                {
                    Text = $"{item.Name} - {item.Price:N0} تومان",
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Fill
                });

                int index = i;
                var remove = CreateButton("حذف", "#555555");
                remove.WidthRequest = 75;

                remove.Clicked += async (s, e) =>
                {
                    cart.RemoveAt(index);
                    await Navigation.PopAsync();
                    await CartPage();
                };

                row.Children.Add(remove);
                layout.Children.Add(row);
            }

            layout.Children.Add(new Label
            {
                Text = $"مجموع خرید: {total:N0} تومان",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(Red)
            });

            var checkout = CreateButton("ثبت سفارش آزمایشی", Red);

            checkout.Clicked += async (s, e) =>
            {
                if (!isLoggedIn)
                {
                    await DisplayAlert("حساب کاربری", "برای ثبت سفارش ابتدا وارد حساب شو.", "باشه");
                    await ShowAccountPage();
                    return;
                }

                bool answer = await DisplayAlert(
                    "ثبت سفارش",
                    $"مبلغ سفارش: {total:N0} تومان\nسفارش آزمایشی ثبت شود؟",
                    "ثبت سفارش",
                    "انصراف");

                if (answer)
                {
                    cart.Clear();

                    await DisplayAlert(
                        "سفارش آزمایشی",
                        "سفارش در این نسخه به‌صورت آزمایشی ثبت شد. پرداخت واقعی انجام نشده است.",
                        "باشه");

                    await Navigation.PopAsync();
                    await CartPage();
                }
            };

            layout.Children.Add(checkout);

            var clear = CreateButton("خالی کردن سبد خرید", "#555555");

            clear.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "حذف محصولات",
                    "سبد خرید خالی شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    cart.Clear();
                    await Navigation.PopAsync();
                    await CartPage();
                }
            };

            layout.Children.Add(clear);
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("سبد خرید", layout));
    }

    private async Task TicketsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("بلیت‌های من"));

        if (!isLoggedIn)
        {
            layout.Children.Add(CreateDescription("برای مشاهده بلیت‌ها ابتدا وارد حساب کاربری شو."));

            var login = CreateButton("ورود به حساب", Red);
            login.Clicked += async (s, e) => await ShowAccountPage();

            layout.Children.Add(login);
        }
        else if (tickets.Count == 0)
        {
            layout.Children.Add(CreateDescription("هنوز بلیتی ثبت نشده است."));
        }
        else
        {
            foreach (var ticket in tickets)
            {
                var box = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = 14,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            CreateHeading(ticket.EventName),
                            CreateDescription($"صندلی: {ticket.Seat}"),
                            CreateDescription($"زمان ثبت: {ticket.Date}"),
                            CreateDescription($"مبلغ: {ticket.Price:N0} تومان"),
                            CreateDescription("وضعیت: رزرو آزمایشی")
                        }
                    }
                };

                layout.Children.Add(box);
            }
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("بلیت‌های من", layout));
    }

    private async Task ShowAccountPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("حساب کاربری همکفیه"));

        if (!isLoggedIn)
        {
            layout.Children.Add(CreateDescription("به حساب کاربری همکفیه خوش آمدی."));

            var login = CreateButton("ورود به حساب", Red);
            login.Clicked += async (s, e) => await LoginPage();

            var register = CreateButton("ثبت‌نام", "#222222");
            register.Clicked += async (s, e) => await RegisterPage();

            layout.Children.Add(login);
            layout.Children.Add(register);
        }
        else
        {
            layout.Children.Add(CreateHeading($"خوش آمدی {userName} {userFamily}"));
            layout.Children.Add(CreateDescription($"شماره موبایل: {userPhone}"));

            var profile = CreateButton("پروفایل من", "#333333");

            profile.Clicked += async (s, e) =>
            {
                await DisplayAlert(
                    "پروفایل",
                    $"نام: {userName}\nنام خانوادگی: {userFamily}\nموبایل: {userPhone}",
                    "باشه");
            };

            layout.Children.Add(profile);

            var favoritesButton = CreateButton("علاقه‌مندی‌های من", "#555555");

            favoritesButton.Clicked += async (s, e) =>
            {
                string result = favorites.Count == 0
                    ? "هنوز موردی به علاقه‌مندی‌ها اضافه نشده است."
                    : string.Join("\n", favorites);

                await DisplayAlert("علاقه‌مندی‌ها", result, "باشه");
            };

            layout.Children.Add(favoritesButton);

            var admin = CreateButton("ورود به پنل مدیریت", Red);
            admin.Clicked += async (s, e) => await AdminLoginPage();

            layout.Children.Add(admin);

            var logout = CreateButton("خروج از حساب", "#777777");

            logout.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "خروج",
                    "از حساب خارج می‌شوی؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    isLoggedIn = false;
                    await Navigation.PopAsync();
                    await ShowAccountPage();
                }
            };

            layout.Children.Add(logout);
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("حساب من", layout));
    }

    private async Task LoginPage()
    {
        var phone = new Entry
        {
            Placeholder = "شماره موبایل",
            Keyboard = Keyboard.Telephone
        };

        var password = new Entry
        {
            Placeholder = "رمز عبور",
            IsPassword = true
        };

        var layout = CreateForm("ورود به همکفیه", phone, password);

        var submit = CreateButton("ورود", Red);
        layout.Children.Add(submit);

        var page = CreatePage("ورود", layout);

        submit.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(phone.Text) ||
                string.IsNullOrWhiteSpace(password.Text))
            {
                await page.DisplayAlert(
                    "اطلاعات ناقص",
                    "شماره موبایل و رمز عبور را وارد کن.",
                    "باشه");
                return;
            }

            if (!(phone.Text.Trim() == userPhone &&
                  password.Text == userPassword &&
                  !string.IsNullOrWhiteSpace(userName)))
            {
                await page.DisplayAlert(
                    "ورود آزمایشی",
                    "شماره موبایل یا رمز عبوری که هنگام ثبت‌نام وارد کردی صحیح نیست.",
                    "باشه");
                return;
            }

            isLoggedIn = true;

            await page.DisplayAlert("ورود موفق", "خوش آمدی!", "باشه");
            await Navigation.PopAsync();
            await ShowAccountPage();
        };

        await Navigation.PushAsync(page);
    }    private async Task RegisterPage()
    {
        var firstName = new Entry { Placeholder = "نام" };
        var lastName = new Entry { Placeholder = "نام خانوادگی" };

        var phone = new Entry
        {
            Placeholder = "شماره موبایل",
            Keyboard = Keyboard.Telephone
        };

        var password = new Entry
        {
            Placeholder = "رمز عبور",
            IsPassword = true
        };

        var layout = CreateForm(
            "ثبت‌نام در همکفیه",
            firstName,
            lastName,
            phone,
            password);

        var submit = CreateButton("ایجاد حساب", Red);
        layout.Children.Add(submit);

        var page = CreatePage("ثبت‌نام", layout);

        submit.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(firstName.Text) ||
                string.IsNullOrWhiteSpace(lastName.Text) ||
                string.IsNullOrWhiteSpace(phone.Text) ||
                string.IsNullOrWhiteSpace(password.Text))
            {
                await page.DisplayAlert("اطلاعات ناقص", "تمام فیلدها را تکمیل کن.", "باشه");
                return;
            }

            userName = firstName.Text.Trim();
            userFamily = lastName.Text.Trim();
            userPhone = phone.Text.Trim();
            userPassword = password.Text;
            isLoggedIn = true;

            await page.DisplayAlert(
                "ثبت‌نام آزمایشی",
                "حساب آزمایشی در حافظه برنامه ایجاد شد.",
                "باشه");

            await Navigation.PopAsync();
            await ShowAccountPage();
        };

        await Navigation.PushAsync(page);
    }

    private async Task AdminLoginPage()
    {
        var password = await DisplayPromptAsync(
            "ورود مدیر",
            "رمز آزمایشی پنل مدیریت را وارد کن:",
            "ورود",
            "انصراف",
            "رمز مدیر",
            -1,
            Keyboard.Numeric);

        if (password != AdminDemoPassword)
        {
            await DisplayAlert("دسترسی", "رمز واردشده صحیح نیست.", "باشه");
            return;
        }

        await AdminPage();
    }

    private async Task AdminPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("پنل مدیریت همکفیه"));
        layout.Children.Add(CreateDescription("مدیریت آزمایشی محصولات و برنامه‌ها"));

        var addEvent = CreateButton("افزودن برنامه جدید", Red);
        addEvent.Clicked += async (s, e) => await AddEventPage();

        var addProduct = CreateButton("افزودن محصول جدید", "#222222");
        addProduct.Clicked += async (s, e) => await AddProductPage();

        var manageEvents = CreateButton("مدیریت برنامه‌ها", "#444444");
        manageEvents.Clicked += async (s, e) => await ManageEventsPage();

        var manageProducts = CreateButton("مدیریت محصولات", "#444444");
        manageProducts.Clicked += async (s, e) => await ManageProductsPage();

        var orders = CreateButton("مشاهده سفارش‌های آزمایشی", "#555555");

        orders.Clicked += async (s, e) =>
        {
            await DisplayAlert(
                "سفارش‌ها",
                "تعداد بلیت‌های آزمایشی: " + tickets.Count +
                "\nتعداد محصولات موجود در سبدها: " + cart.Count,
                "باشه");
        };

        layout.Children.Add(addEvent);
        layout.Children.Add(addProduct);
        layout.Children.Add(manageEvents);
        layout.Children.Add(manageProducts);
        layout.Children.Add(orders);
        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("پنل مدیریت", layout));
    }

    private async Task AddEventPage()
    {
        var name = new Entry { Placeholder = "نام برنامه" };

        var category = new Picker { Title = "انتخاب دسته‌بندی" };
        category.Items.Add("تئاتر");
        category.Items.Add("سینما ترس");
        category.Items.Add("اتاق فرار");
        category.Items.Add("همایش و رویداد");

        var description = new Entry { Placeholder = "توضیحات و زمان برنامه" };

        var price = new Entry
        {
            Placeholder = "قیمت بلیت به تومان",
            Keyboard = Keyboard.Numeric
        };

        var layout = CreateForm("افزودن برنامه", name, category, description, price);
        var save = CreateButton("ذخیره برنامه", Red);
        layout.Children.Add(save);

        var page = CreatePage("افزودن برنامه", layout);

        save.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                category.SelectedItem == null ||
                string.IsNullOrWhiteSpace(price.Text) ||
                !decimal.TryParse(price.Text, out decimal amount))
            {
                await page.DisplayAlert("خطا", "اطلاعات برنامه را کامل و صحیح وارد کن.", "باشه");
                return;
            }

            events.Add(new EventItem(
                name.Text.Trim(),
                category.SelectedItem.ToString()!,
                description.Text?.Trim() ?? "",
                amount));

            await page.DisplayAlert("مدیریت", "برنامه جدید اضافه شد.", "باشه");
            await Navigation.PopAsync();
        };

        await Navigation.PushAsync(page);
    }

    private async Task AddProductPage()
    {
        var name = new Entry { Placeholder = "نام محصول" };
        var description = new Entry { Placeholder = "توضیحات محصول" };

        var price = new Entry
        {
            Placeholder = "قیمت به تومان",
            Keyboard = Keyboard.Numeric
        };

        var layout = CreateForm("افزودن محصول", name, description, price);
        var save = CreateButton("ذخیره محصول", Red);
        layout.Children.Add(save);

        var page = CreatePage("افزودن محصول", layout);

        save.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                string.IsNullOrWhiteSpace(price.Text) ||
                !decimal.TryParse(price.Text, out decimal amount))
            {
                await page.DisplayAlert("خطا", "نام و قیمت معتبر وارد کن.", "باشه");
                return;
            }

            products.Add(new Product(
                name.Text.Trim(),
                description.Text?.Trim() ?? "",
                amount));

            await page.DisplayAlert("مدیریت", "محصول اضافه شد.", "باشه");
            await Navigation.PopAsync();
        };

        await Navigation.PushAsync(page);
    }

    private async Task ManageEventsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("مدیریت برنامه‌ها"));

        foreach (var item in events.ToList())
        {
            var row = new VerticalStackLayout { Spacing = 6 };

            row.Children.Add(CreateHeading(item.Name));
            row.Children.Add(CreateDescription(item.Category));

            var delete = CreateButton("حذف برنامه", "#555555");

            delete.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "حذف برنامه",
                    "این برنامه حذف شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    events.Remove(item);
                    await Navigation.PopAsync();
                    await ManageEventsPage();
                }
            };

            row.Children.Add(delete);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 10,
                Content = row
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("مدیریت برنامه‌ها", layout));
    }

    private async Task ManageProductsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("مدیریت محصولات"));

        foreach (var item in products.ToList())
        {
            var row = new VerticalStackLayout { Spacing = 6 };

            row.Children.Add(CreateHeading(item.Name));
            row.Children.Add(CreateDescription($"{item.Price:N0} تومان"));

            var delete = CreateButton("حذف محصول", "#555555");

            delete.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "حذف محصول",
                    "این محصول حذف شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    products.Remove(item);
                    await Navigation.PopAsync();
                    await ManageProductsPage();
                }
            };

            row.Children.Add(delete);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 10,
                Content = row
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("مدیریت محصولات", layout));
    }

    private VerticalStackLayout CreateLayout()
    {
        return new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 16
        };
    }

    private VerticalStackLayout CreateForm(string heading, params View[] controls)
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading(heading));

        foreach (var control in controls)
            layout.Children.Add(control);

        return layout;
    }

    private ContentPage CreatePage(string title, View content)
    {
        return new ContentPage
        {
            Title = title,
            FlowDirection = FlowDirection.RightToLeft,
            BackgroundColor = Color.FromArgb("#F4F4F6"),
            Content = new ScrollView { Content = content }
        };
    }

    private Label CreateHeading(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(Red)
        };
    }

    private Label CreateDescription(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 14,
            TextColor = Colors.Gray
        };
    }

    private Button CreateButton(string text, string color)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Color.FromArgb(color),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50,
            FontSize = 15
        };
    }

    private Button CreateBackButton()
    {
        var back = CreateButton("بازگشت", "#333333");

        back.Clicked += async (s, e) =>
        {
            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
        };

        return back;
    }

    private class Product
    {
        public string Name { get; }
        public string Description { get; }
        public decimal Price { get; }

        public Product(string name, string description, decimal price)
        {
            Name = name;
            Description = description;
            Price = price;
        }
    }

    private class EventItem
    {
        public string Name { get; }
        public string Category { get; }
        public string Description { get; }
        public decimal Price { get; }

        public EventItem(string name, string category, string description, decimal price)
        {
            Name = name;
            Category = category;
            Description = description;
            Price = price;
        }
    }

    private class CartItem
    {
        public string Name { get; }
        public decimal Price { get; }

        public CartItem(string name, decimal price)
        {
            Name = name;
            Price = price;
        }
    }

    private class TicketItem
    {
        public string EventName { get; }
        public string Seat { get; }
        public string Date { get; }
        public decimal Price { get; }

        public TicketItem(string eventName, string seat, string date, decimal price)
        {
            EventName = eventName;
            Seat = seat;
            Date = date;
            Price = price;
        }
    }
}