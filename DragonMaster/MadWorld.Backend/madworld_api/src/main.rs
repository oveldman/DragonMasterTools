#[macro_use] extern crate rocket;

#[get("/")]
fn index() -> &'static str {
    "Hello, world!"
}

#[rocket::main]
async fn main() {
    let _ = rocket::build().mount("/", routes![index]).launch().await;
}
