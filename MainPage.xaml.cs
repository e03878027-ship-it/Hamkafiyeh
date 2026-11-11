
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Hamkafiyeh;

public partial class MainPage : ContentPage
{
    private string userName = "";
    private string userFamily = "";
    private string userPhone = "";
    private string userPassword = "";
    private bool isLoggedIn;

    private readonly List<Product> products = new();
    private readonly List<EventItem> events = new();
    private readonly List<CartItem> cart = new();
    private readonly List<TicketItem> tickets = new();
    private readonly HashSet<string> favorites = new();

    private const string Red = "#D20F1D";
    private const string AdminDemoPassword = "2468";

    public MainPage()
    {
        InitializeComponent();

        products.Add(new Product("محصول ویژه همکفیه", "محصول اختصاصی فروشگاه", 150000));
        products.Add(new Product("یادگاری همکفیه", "یادگاری از تجربه‌های همکفیه", 200000));
        products.Add(new Product("محصول اختصاصی", "محصول ویژه علاقه‌مندان", 100000));

        AddDemoEvent("اجرای ویژه همکفیه", "تئاتر", "زمان اجرا به‌زودی اعلام می‌شود", 150000);
        AddDemoEvent("تجربه سینما ترس", "سینما ترس", "زمان سانس به‌زودی اعلام می‌شود", 120000);
        AddDemoEvent("ماجراجویی اتاق فرار", "اتاق فرار", "زمان سانس به‌زودی اعلام می‌شود", 180000);
        AddDemoEvent("دورهمی همکفیه", "همایش و رویداد", "زمان برگزاری به‌زودی اعلام می‌شود", 100000);
    }

    private void AddDemoEvent(string name, string category, string description, decimal price)
    {
        var item = new EventItem(name, category, description, "");

        item.Sessions.Add(new ShowSession(
            "سانس اول",
            price,
            price,
            price,
            price));

        events.Add(item);
    }

    // ---------------- صفحه اصلی و جستجو ----------------

    private async void HomeClicked(object sender, TappedEventArgs e)
    {
        await DisplayAlert("همکفیه", "شما در صفحه اصلی هستید.", "باشه");
    }

    private async void SearchClicked(object sender, EventArgs e)
    {
        string search = SearchInput.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(search))
            search = await DisplayPromptAsync("جستجو", "نام بخش موردنظر را وارد کن:");

        if (!string.IsNullOrWhiteSpace(search))
            await SearchResult(search);
    }

    private async void SearchTabClicked(object sender, TappedEventArgs e)
    {
        string search = await DisplayPromptAsync("جستجو", "نام بخش موردنظر را وارد کن:");

        if (!string.IsNullOrWhiteSpace(search))
        {
            SearchInput.Text = search;
            await SearchResult(search);
        }
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

    private Task TheaterPage() => CategoryPage("تئاتر", "theater.jpg");
    private Task HorrorPage() => CategoryPage("سینما ترس", "horror.jpg");
    private Task EscapePage() => CategoryPage("اتاق فرار", "escape.jpg");

    // ---------------- نمایش برنامه‌ها ----------------

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

        var items = events.Where(x => x.Category == category).ToList();

        if (items.Count == 0)
            layout.Children.Add(CreateDescription("فعلاً برنامه‌ای در این بخش ثبت نشده است."));

        foreach (var item in items)
            layout.Children.Add(CreateEventCard(item));

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

        var items = events.Where(x => x.Category == "همایش و رویداد").ToList();

        if (items.Count == 0)
            layout.Children.Add(CreateDescription("هنوز رویدادی ثبت نشده است."));

        foreach (var item in items)
            layout.Children.Add(CreateEventCard(item));

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("رویدادها", layout));
    }

    private Border CreateEventCard(EventItem item)
    {
        var content = new VerticalStackLayout
        {
            Padding = 12,
            Spacing = 9
        };

        if (!string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
        {
            content.Children.Add(new Image
            {
                Source = ImageSource.FromFile(item.ImagePath),
                HeightRequest = 175,
                Aspect = Aspect.AspectFill
            });
        }

        content.Children.Add(CreateHeading(item.Name));
        content.Children.Add(CreateDescription(item.Category));
        content.Children.Add(CreateDescription(item.Description));

        decimal minPrice = item.Sessions.Count == 0
            ? 0
            : item.Sessions.Min(x => x.MinimumPrice);

        content.Children.Add(new Label
        {
            Text = item.Sessions.Count == 0
                ? "قیمت هنوز مشخص نشده"
                : $"شروع قیمت بلیت: {minPrice:N0} تومان",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(Red)
        });

        var reserve = CreateButton("انتخاب سانس و رزرو بلیت", Red);

        reserve.Clicked += async (s, e) =>
        {
            if (!isLoggedIn)
            {
                await DisplayAlert("ورود به حساب", "برای رزرو ابتدا وارد حساب کاربری شو.", "باشه");
                await ShowAccountPage();
                return;
            }

            await ChooseSessionPage(item);
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

    // ---------------- انتخاب سانس ----------------

    private async Task ChooseSessionPage(EventItem item)
    {
        if (item.Sessions.Count == 0)
        {
            await DisplayAlert("سانس", "برای این برنامه هنوز سانسی تعریف نشده است.", "باشه");
            return;
        }

        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("انتخاب سانس"));
        layout.Children.Add(CreateDescription(item.Name));

        foreach (var session in item.Sessions)
        {
            var box = new VerticalStackLayout
            {
                Spacing = 7,
                Padding = 12
            };

            box.Children.Add(CreateHeading(session.Name));
            box.Children.Add(CreateDescription(
                $"VIP: {session.PriceA:N0} تومان"));

            box.Children.Add(CreateDescription(
                $"ردیف B: {session.PriceB:N0} تومان"));

            box.Children.Add(CreateDescription(
                $"ردیف C: {session.PriceC:N0} تومان"));

            box.Children.Add(CreateDescription(
                $"بالکن D: {session.PriceD:N0} تومان"));

            var choose = CreateButton("انتخاب این سانس", Red);

            choose.Clicked += async (s, e) =>
            {
                await SeatSelectionPage(item, session);
            };

            box.Children.Add(choose);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 8,
                Content = box
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("انتخاب سانس", layout));
    }

    // ---------------- انتخاب صندلی ----------------

    private async Task SeatSelectionPage(EventItem item, ShowSession session)
    {
        var selectedSeats = new HashSet<string>();
        var layout = CreateLayout();

        var selectedLabel = new Label
        {
            Text = "هنوز صندلی انتخاب نشده است.",
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var totalLabel = new Label
        {
            Text = "مبلغ کل: ۰ تومان",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(Red),
            HorizontalTextAlignment = TextAlignment.Center
        };

        decimal GetSeatPrice(string seat)
        {
            if (seat.StartsWith("A")) return session.PriceA;
            if (seat.StartsWith("B")) return session.PriceB;
            if (seat.StartsWith("C")) return session.PriceC;
            if (seat.StartsWith("D")) return session.PriceD;
            return 0;
        }

        void RefreshSelection()
        {
            selectedLabel.Text = selectedSeats.Count == 0
                ? "هنوز صندلی انتخاب نشده است."
                : "صندلی‌ها: " + string.Join("، ", selectedSeats.OrderBy(x => x));

            decimal total = selectedSeats.Sum(GetSeatPrice);
            totalLabel.Text = $"مبلغ کل: {total:N0} تومان";
        }

        layout.Children.Add(CreateHeading("انتخاب صندلی"));
        layout.Children.Add(CreateDescription($"برنامه: {item.Name}"));
        layout.Children.Add(CreateDescription($"سانس: {session.Name}"));

        layout.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#333333"),
            StrokeThickness = 0,
            Padding = 12,
            Content = new Label
            {
                Text = "صحنه نمایش",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Center
            }
        });

        var legend = new HorizontalStackLayout
        {
            Spacing = 12,
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
            ColumnSpacing = 5,
            RowSpacing = 18,
            Padding = 2
        };

        for (int i = 0; i < 3; i++)
            seatMap.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var left = new VerticalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        left.Children.Add(CreateDescription("ردیف C"));
        left.Children.Add(CreateSeatButton("C1"));
        left.Children.Add(CreateSeatButton("C2"));
        left.Children.Add(CreateSeatButton("C3"));

        seatMap.Add(left, 0, 0);
        Grid.SetRowSpan(left, 3);

        var center = new VerticalStackLayout
        {
            Spacing = 15,
            HorizontalOptions = LayoutOptions.Center
        };

        center.Children.Add(CreateHeading("ردیف A - VIP"));

        var rowA = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };

        rowA.Children.Add(CreateSeatButton("A1"));
        rowA.Children.Add(CreateSeatButton("A2"));
        center.Children.Add(rowA);

        center.Children.Add(CreateHeading("ردیف B"));

        var rowB = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center
        };

        rowB.Children.Add(CreateSeatButton("B1"));
        rowB.Children.Add(CreateSeatButton("B2"));
        rowB.Children.Add(CreateSeatButton("B3"));
        center.Children.Add(rowB);

        seatMap.Add(center, 1, 0);
        Grid.SetRowSpan(center, 3);

        var balcony = new VerticalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        balcony.Children.Add(CreateDescription("بالکن"));
        balcony.Children.Add(CreateSeatButton("D1"));

        seatMap.Add(balcony, 2, 0);
        Grid.SetRowSpan(balcony, 3);

        layout.Children.Add(seatMap);
        layout.Children.Add(selectedLabel);
        layout.Children.Add(totalLabel);

        Button CreateSeatButton(string seatName)
        {
            bool reserved = session.ReservedSeats.Contains(seatName);

            var button = new Button
            {
                Text = seatName,
                WidthRequest = 58,
                HeightRequest = 48,
                Padding = 2,
                FontSize = 13,
                CornerRadius = 10,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb(reserved ? Red : "#2E9D62"),
                IsEnabled = !reserved
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

                RefreshSelection();
            };

            return button;
        }

        var confirm = CreateButton("تأیید صندلی‌ها و ثبت رزرو", Red);

        confirm.Clicked += async (sender, args) =>
        {
            if (selectedSeats.Count == 0)
            {
                await DisplayAlert("انتخاب صندلی", "حداقل یک صندلی انتخاب کن.", "باشه");
                return;
            }

            if (selectedSeats.Any(x => session.ReservedSeats.Contains(x)))
            {
                await DisplayAlert("صندلی ناموجود", "یکی از صندلی‌ها قبلاً رزرو شده است.", "باشه");
                return;
            }

            decimal total = selectedSeats.Sum(GetSeatPrice);
            string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            foreach (var seat in selectedSeats)
            {
                decimal price = GetSeatPrice(seat);

                session.ReservedSeats.Add(seat);
                tickets.Add(new TicketItem(item.Name, session.Name, seat, date, price));
            }

            string seats = string.Join("، ", selectedSeats.OrderBy(x => x));

            await DisplayAlert(
                "رزرو آزمایشی",
                $"برنامه: {item.Name}\nسانس: {session.Name}\nصندلی‌ها: {seats}\nمبلغ کل: {total:N0} تومان\n\nرزرو فقط در حافظه همین اجرای برنامه ثبت شد.",
                "باشه");

            await Navigation.PopAsync();
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
                    CornerRadius = 3
                },
                new Label
                {
                    Text = text,
                    FontSize = 12,
                    TextColor = Colors.Black
                }
            }
        };
    }

    // ---------------- انتخاب تصویر برنامه ----------------

    private async Task<string> PickEventImage()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "انتخاب پوستر برنامه",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
                return "";

            string extension = Path.GetExtension(result.FileName);

            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destination = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using Stream source = await result.OpenReadAsync();
            using FileStream target = File.Create(destination);

            await source.CopyToAsync(target);

            return destination;
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطای تصویر", ex.Message, "باشه");
            return "";
        }
    }

    // ---------------- فروشگاه ----------------

    private async Task ShopPage()
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

        var add = CreateButton("افزودن به سبد خرید", Red);

        add.Clicked += async (s, e) =>
        {
            cart.Add(new CartItem(product.Name, product.Price));
            await DisplayAlert("سبد خرید", "محصول اضافه شد.", "باشه");
        };

        content.Children.Add(add);

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
            decimal total = cart.Sum(x => x.Price);

            foreach (var item in cart.ToList())
            {
                var row = new VerticalStackLayout { Spacing = 5 };

                row.Children.Add(CreateDescription($"{item.Name} - {item.Price:N0} تومان"));

                var remove = CreateButton("حذف محصول", "#555555");

                remove.Clicked += async (s, e) =>
                {
                    cart.Remove(item);
                    await Navigation.PopAsync();
                    await CartPage();
                };

                row.Children.Add(remove);
                layout.Children.Add(row);
            }

            layout.Children.Add(CreateHeading($"مجموع: {total:N0} تومان"));

            var checkout = CreateButton("ثبت سفارش آزمایشی", Red);

            checkout.Clicked += async (s, e) =>
            {
                if (!isLoggedIn)
                {
                    await DisplayAlert("حساب کاربری", "ابتدا وارد حساب شو.", "باشه");
                    await ShowAccountPage();
                    return;
                }

                bool answer = await DisplayAlert(
                    "ثبت سفارش",
                    $"مبلغ سفارش: {total:N0} تومان\nثبت شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    cart.Clear();

                    await DisplayAlert(
                        "سفارش آزمایشی",
                        "سفارش ثبت شد؛ پرداخت واقعی انجام نشده است.",
                        "باشه");

                    await Navigation.PopAsync();
                    await CartPage();
                }
            };

            layout.Children.Add(checkout);
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("سبد خرید", layout));
    }

    // ---------------- بلیت‌ها ----------------

    private async Task TicketsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("بلیت‌های من"));

        if (!isLoggedIn)
        {
            layout.Children.Add(CreateDescription("برای مشاهده بلیت‌ها ابتدا وارد حساب شو."));

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
                layout.Children.Add(new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = 14,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 7,
                        Children =
                        {
                            CreateHeading(ticket.EventName),
                            CreateDescription($"سانس: {ticket.SessionName}"),
                            CreateDescription($"صندلی: {ticket.Seat}"),
                            CreateDescription($"زمان ثبت: {ticket.Date}"),
                            CreateDescription($"مبلغ: {ticket.Price:N0} تومان"),
                            CreateDescription("وضعیت: رزرو آزمایشی")
                        }
                    }
                });
            }
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("بلیت‌های من", layout));
    }

    // ---------------- حساب کاربری ----------------

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

            var favoritesButton = CreateButton("علاقه‌مندی‌های من", "#555555");

            favoritesButton.Clicked += async (s, e) =>
            {
                string result = favorites.Count == 0
                    ? "هنوز موردی اضافه نشده است."
                    : string.Join("\n", favorites);

                await DisplayAlert("علاقه‌مندی‌ها", result, "باشه");
            };

            var admin = CreateButton("ورود به پنل مدیریت", Red);
            admin.Clicked += async (s, e) => await AdminLoginPage();

            var logout = CreateButton("خروج از حساب", "#777777");

            logout.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert("خروج", "از حساب خارج می‌شوی؟", "بله", "خیر");

                if (answer)
                {
                    isLoggedIn = false;
                    await Navigation.PopAsync();
                    await ShowAccountPage();
                }
            };

            layout.Children.Add(profile);
            layout.Children.Add(favoritesButton);
            layout.Children.Add(admin);
            layout.Children.Add(logout);
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("حساب من", layout));
    }

    private async Task LoginPage()
    {
        var phone = new Entry { Placeholder = "شماره موبایل", Keyboard = Keyboard.Telephone };
        var password = new Entry { Placeholder = "رمز عبور", IsPassword = true };

        var layout = CreateForm("ورود به همکفیه", phone, password);
        var submit = CreateButton("ورود", Red);

        layout.Children.Add(submit);

        var page = CreatePage("ورود", layout);

        submit.Clicked += async (s, e) =>
        {
            if (phone.Text?.Trim() != userPhone ||
                password.Text != userPassword ||
                string.IsNullOrWhiteSpace(userName))
            {
                await page.DisplayAlert("ورود ناموفق", "شماره موبایل یا رمز صحیح نیست.", "باشه");
                return;
            }

            isLoggedIn = true;

            await page.DisplayAlert("ورود موفق", "خوش آمدی!", "باشه");
            await Navigation.PopAsync();
            await ShowAccountPage();
        };

        await Navigation.PushAsync(page);
    }

    private async Task RegisterPage()
    {
        var firstName = new Entry { Placeholder = "نام" };
        var lastName = new Entry { Placeholder = "نام خانوادگی" };
        var phone = new Entry { Placeholder = "شماره موبایل", Keyboard = Keyboard.Telephone };
        var password = new Entry { Placeholder = "رمز عبور", IsPassword = true };

        var layout = CreateForm("ثبت‌نام در همکفیه", firstName, lastName, phone, password);
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

            await page.DisplayAlert("ثبت‌نام آزمایشی", "حساب ایجاد شد.", "باشه");
            await Navigation.PopAsync();
            await ShowAccountPage();
        };

        await Navigation.PushAsync(page);
    }

    // ---------------- پنل مدیریت ----------------

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
        layout.Children.Add(CreateDescription("رمز آزمایشی مدیر: 2468"));

        var addEvent = CreateButton("افزودن برنامه جدید", Red);
        addEvent.Clicked += async (s, e) => await AddEventPage();

        var addProduct = CreateButton("افزودن محصول جدید", "#222222");
        addProduct.Clicked += async (s, e) => await AddProductPage();

        var manageEvents = CreateButton("مدیریت برنامه‌ها، پوسترها و سانس‌ها", "#444444");
        manageEvents.Clicked += async (s, e) => await ManageEventsPage();

        var manageProducts = CreateButton("مدیریت محصولات", "#444444");
        manageProducts.Clicked += async (s, e) => await ManageProductsPage();

        var orders = CreateButton("مشاهده سفارش‌های آزمایشی", "#555555");

        orders.Clicked += async (s, e) =>
        {
            await DisplayAlert(
                "سفارش‌ها",
                $"تعداد بلیت‌های آزمایشی: {tickets.Count}\nتعداد محصولات در سبد: {cart.Count}",
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
        var sessionName = new Entry { Placeholder = "نام سانس، مثلاً جمعه ساعت ۲۰" };

        var priceA = new Entry { Placeholder = "قیمت VIP به تومان", Keyboard = Keyboard.Numeric };
        var priceB = new Entry { Placeholder = "قیمت ردیف B به تومان", Keyboard = Keyboard.Numeric };
        var priceC = new Entry { Placeholder = "قیمت ردیف C به تومان", Keyboard = Keyboard.Numeric };
        var priceD = new Entry { Placeholder = "قیمت بالکن D به تومان", Keyboard = Keyboard.Numeric };

        string imagePath = "";

        var imageLabel = CreateDescription("هنوز پوستری انتخاب نشده است.");

        var chooseImage = CreateButton("انتخاب پوستر از گالری گوشی", "#444444");

        chooseImage.Clicked += async (s, e) =>
        {
            string selected = await PickEventImage();

            if (!string.IsNullOrWhiteSpace(selected))
            {
                imagePath = selected;
                imageLabel.Text = "پوستر با موفقیت انتخاب شد.";
            }
        };

        var layout = CreateForm(
            "افزودن برنامه جدید",
            name,
            category,
            description,
            sessionName,
            CreateHeading("قیمت صندلی‌ها"),
            priceA,
            priceB,
            priceC,
            priceD);

        layout.Children.Add(chooseImage);
        layout.Children.Add(imageLabel);

        var save = CreateButton("ذخیره برنامه", Red);
        layout.Children.Add(save);

        var page = CreatePage("افزودن برنامه", layout);

        save.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                category.SelectedItem == null ||
                !TryPrice(priceA.Text, out decimal a) ||
                !TryPrice(priceB.Text, out decimal b) ||
                !TryPrice(priceC.Text, out decimal c) ||
                !TryPrice(priceD.Text, out decimal d))
            {
                await page.DisplayAlert(
                    "اطلاعات ناقص",
                    "نام، دسته‌بندی و هر چهار قیمت را درست وارد کن.",
                    "باشه");
                return;
            }

            string sn = string.IsNullOrWhiteSpace(sessionName.Text)
                ? "سانس اول"
                : sessionName.Text.Trim();

            var item = new EventItem(
                name.Text.Trim(),
                category.SelectedItem.ToString()!,
                description.Text?.Trim() ?? "",
                imagePath);

            item.Sessions.Add(new ShowSession(sn, a, b, c, d));
            events.Add(item);

            await page.DisplayAlert("مدیریت", "برنامه و سانس آن اضافه شد.", "باشه");
            await Navigation.PopAsync();
        };

        await Navigation.PushAsync(page);
    }

    private async Task AddSessionPage(EventItem item)
    {
        var name = new Entry { Placeholder = "نام سانس یا تاریخ و ساعت" };

        var priceA = new Entry { Placeholder = "قیمت VIP", Keyboard = Keyboard.Numeric };
        var priceB = new Entry { Placeholder = "قیمت ردیف B", Keyboard = Keyboard.Numeric };
        var priceC = new Entry { Placeholder = "قیمت ردیف C", Keyboard = Keyboard.Numeric };
        var priceD = new Entry { Placeholder = "قیمت بالکن D", Keyboard = Keyboard.Numeric };

        var layout = CreateForm(
            $"افزودن سانس برای {item.Name}",
            name,
            CreateHeading("قیمت هر ردیف"),
            priceA,
            priceB,
            priceC,
            priceD);

        var save = CreateButton("ذخیره سانس", Red);
        layout.Children.Add(save);

        var page = CreatePage("افزودن سانس", layout);

        save.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                !TryPrice(priceA.Text, out decimal a) ||
                !TryPrice(priceB.Text, out decimal b) ||
                !TryPrice(priceC.Text, out decimal c) ||
                !TryPrice(priceD.Text, out decimal d))
            {
                await page.DisplayAlert("خطا", "نام سانس و چهار قیمت را صحیح وارد کن.", "باشه");
                return;
            }

            item.Sessions.Add(new ShowSession(name.Text.Trim(), a, b, c, d));

            await page.DisplayAlert("مدیریت", "سانس جدید اضافه شد.", "باشه");
            await Navigation.PopAsync();
            await ManageEventsPage();
        };

        await Navigation.PushAsync(page);
    }

    private async Task AddProductPage()
    {
        var name = new Entry { Placeholder = "نام محصول" };
        var description = new Entry { Placeholder = "توضیحات محصول" };
        var price = new Entry { Placeholder = "قیمت به تومان", Keyboard = Keyboard.Numeric };

        var layout = CreateForm("افزودن محصول", name, description, price);
        var save = CreateButton("ذخیره محصول", Red);

        layout.Children.Add(save);

        var page = CreatePage("افزودن محصول", layout);

        save.Clicked += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                !TryPrice(price.Text, out decimal amount))
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

        if (events.Count == 0)
            layout.Children.Add(CreateDescription("برنامه‌ای ثبت نشده است."));

        foreach (var item in events.ToList())
        {
            var box = new VerticalStackLayout
            {
                Spacing = 8,
                Padding = 10
            };

            if (!string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
            {
                box.Children.Add(new Image
                {
                    Source = ImageSource.FromFile(item.ImagePath),
                    HeightRequest = 150,
                    Aspect = Aspect.AspectFill
                });
            }

            box.Children.Add(CreateHeading(item.Name));
            box.Children.Add(CreateDescription(item.Category));
            box.Children.Add(CreateDescription($"تعداد سانس‌ها: {item.Sessions.Count}"));

            var changeImage = CreateButton("تغییر پوستر برنامه", "#444444");

            changeImage.Clicked += async (s, e) =>
            {
                string selected = await PickEventImage();

                if (!string.IsNullOrWhiteSpace(selected))
                {
                    item.ImagePath = selected;
                    await DisplayAlert("تصویر", "پوستر برنامه تغییر کرد.", "باشه");
                    await Navigation.PopAsync();
                    await ManageEventsPage();
                }
            };

            var deleteImage = CreateButton("حذف پوستر برنامه", "#555555");

            deleteImage.Clicked += async (s, e) =>
            {
                item.ImagePath = "";
                await DisplayAlert("تصویر", "پوستر از برنامه حذف شد.", "باشه");
                await Navigation.PopAsync();
                await ManageEventsPage();
            };

            var addSession = CreateButton("افزودن سانس جدید و قیمت‌ها", Red);
            addSession.Clicked += async (s, e) => await AddSessionPage(item);

            var showSessions = CreateButton("مشاهده و حذف سانس‌ها", "#333333");
            showSessions.Clicked += async (s, e) => await ManageSessionsPage(item);

            var delete = CreateButton("حذف کل برنامه", "#777777");

            delete.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "حذف برنامه",
                    "این برنامه و سانس‌های آن حذف شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    events.Remove(item);
                    await Navigation.PopAsync();
                    await ManageEventsPage();
                }
            };

            box.Children.Add(changeImage);
            box.Children.Add(deleteImage);
            box.Children.Add(addSession);
            box.Children.Add(showSessions);
            box.Children.Add(delete);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 8,
                Content = box
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("مدیریت برنامه‌ها", layout));
    }

    private async Task ManageSessionsPage(EventItem item)
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading($"سانس‌های {item.Name}"));

        foreach (var session in item.Sessions.ToList())
        {
            var box = new VerticalStackLayout { Spacing = 7, Padding = 10 };

            box.Children.Add(CreateHeading(session.Name));
            box.Children.Add(CreateDescription($"VIP: {session.PriceA:N0} تومان"));
            box.Children.Add(CreateDescription($"ردیف B: {session.PriceB:N0} تومان"));
            box.Children.Add(CreateDescription($"ردیف C: {session.PriceC:N0} تومان"));
            box.Children.Add(CreateDescription($"بالکن D: {session.PriceD:N0} تومان"));
            box.Children.Add(CreateDescription($"صندلی‌های رزروشده: {session.ReservedSeats.Count}"));

            var delete = CreateButton("حذف این سانس", "#555555");

            delete.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert(
                    "حذف سانس",
                    "این سانس حذف شود؟",
                    "بله",
                    "خیر");

                if (answer)
                {
                    item.Sessions.Remove(session);
                    await Navigation.PopAsync();
                    await ManageSessionsPage(item);
                }
            };

            box.Children.Add(delete);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 8,
                Content = box
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("مدیریت سانس‌ها", layout));
    }

    private async Task ManageProductsPage()
    {
        var layout = CreateLayout();

        layout.Children.Add(CreateHeading("مدیریت محصولات"));

        foreach (var item in products.ToList())
        {
            var box = new VerticalStackLayout { Spacing = 6, Padding = 10 };

            box.Children.Add(CreateHeading(item.Name));
            box.Children.Add(CreateDescription($"{item.Price:N0} تومان"));

            var delete = CreateButton("حذف محصول", "#555555");

            delete.Clicked += async (s, e) =>
            {
                bool answer = await DisplayAlert("حذف محصول", "این محصول حذف شود؟", "بله", "خیر");

                if (answer)
                {
                    products.Remove(item);
                    await Navigation.PopAsync();
                    await ManageProductsPage();
                }
            };

            box.Children.Add(delete);

            layout.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = 10,
                Content = box
            });
        }

        layout.Children.Add(CreateBackButton());

        await Navigation.PushAsync(CreatePage("مدیریت محصولات", layout));
    }

    // ---------------- ابزارهای ساخت صفحه ----------------

    private bool TryPrice(string? text, out decimal price)
    {
        return decimal.TryParse(text, out price) && price >= 0;
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

    // ---------------- مدل اطلاعات ----------------

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
        public string ImagePath { get; set; }

        public List<ShowSession> Sessions { get; } = new();

        public EventItem(string name, string category, string description, string imagePath)
        {
            Name = name;
            Category = category;
            Description = description;
            ImagePath = imagePath;
        }
    }

    private class ShowSession
    {
        public string Name { get; }
        public decimal PriceA { get; }
        public decimal PriceB { get; }
        public decimal PriceC { get; }
        public decimal PriceD { get; }

        public HashSet<string> ReservedSeats { get; } = new();

        public decimal MinimumPrice => new[]
        {
            PriceA, PriceB, PriceC, PriceD
        }.Min();

        public ShowSession(
            string name,
            decimal priceA,
            decimal priceB,
            decimal priceC,
            decimal priceD)
        {
            Name = name;
            PriceA = priceA;
            PriceB = priceB;
            PriceC = priceC;
            PriceD = priceD;
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
        public string SessionName { get; }
        public string Seat { get; }
        public string Date { get; }
        public decimal Price { get; }

        public TicketItem(
            string eventName,
            string sessionName,
            string seat,
            string date,
            decimal price)
        {
            EventName = eventName;
            SessionName = sessionName;
            Seat = seat;
            Date = date;
            Price = price;
        }
    }
}
