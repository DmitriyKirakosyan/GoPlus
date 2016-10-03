namespace GoPlus

open Xamarin.Forms
open Menu

type App() =
    inherit Application()

    let stack = StackLayout(VerticalOptions = LayoutOptions.Center)
    let label = Label(XAlign = TextAlignment.Center, Text = "Welcome to F# Xamarin.Forms!")
    do
        stack.Children.Add(label)
        let navPage = NavigationPage( Menu() )
        base.MainPage <- navPage
