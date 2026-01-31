using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoffeeShops.Data.Migrations
{
    /// <inheritdoc />
    public partial class IntRoleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id_category = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_category", x => x.id_category);
                });

            migrationBuilder.CreateTable(
                name: "drinks",
                columns: table => new
                {
                    id_drink = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_drink", x => x.id_drink);
                });

            migrationBuilder.CreateTable(
                name: "loyaltyprograms",
                columns: table => new
                {
                    id_lp = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_lp", x => x.id_lp);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_role", x => x.id_role);
                });

            migrationBuilder.CreateTable(
                name: "drinkscategory",
                columns: table => new
                {
                    id_drink = table.Column<Guid>(type: "uuid", nullable: false),
                    id_category = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drinkscategory", x => new { x.id_drink, x.id_category });
                    table.ForeignKey(
                        name: "FK_drinkscategory_categories_id_category",
                        column: x => x.id_category,
                        principalTable: "categories",
                        principalColumn: "id_category",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_drinkscategory_drinks_id_drink",
                        column: x => x.id_drink,
                        principalTable: "drinks",
                        principalColumn: "id_drink",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id_company = table.Column<Guid>(type: "uuid", nullable: false),
                    id_lp = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    website = table.Column<string>(type: "varchar(256)", nullable: false),
                    amountcoffeeshops = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_company", x => x.id_company);
                    table.ForeignKey(
                        name: "FK_companies_loyaltyprograms_id_lp",
                        column: x => x.id_lp,
                        principalTable: "loyaltyprograms",
                        principalColumn: "id_lp",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id_user = table.Column<Guid>(type: "uuid", nullable: false),
                    id_role = table.Column<int>(type: "int", nullable: false),
                    login = table.Column<string>(type: "varchar(64)", nullable: false),
                    password = table.Column<string>(type: "varchar(128)", nullable: false),
                    birthdate = table.Column<DateTime>(type: "date", nullable: false),
                    email = table.Column<string>(type: "varchar(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_user", x => x.id_user);
                    table.ForeignKey(
                        name: "FK_users_roles_id_role",
                        column: x => x.id_role,
                        principalTable: "roles",
                        principalColumn: "id_role",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coffeeshops",
                columns: table => new
                {
                    id_coffeeshop = table.Column<Guid>(type: "uuid", nullable: false),
                    id_company = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "varchar(256)", nullable: false),
                    workinghours = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id_coffeeshop", x => x.id_coffeeshop);
                    table.ForeignKey(
                        name: "FK_coffeeshops_companies_id_company",
                        column: x => x.id_company,
                        principalTable: "companies",
                        principalColumn: "id_company",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    id_drink = table.Column<Guid>(type: "uuid", nullable: false),
                    id_company = table.Column<Guid>(type: "uuid", nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu", x => new { x.id_drink, x.id_company });
                    table.ForeignKey(
                        name: "FK_menu_companies_id_company",
                        column: x => x.id_company,
                        principalTable: "companies",
                        principalColumn: "id_company",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_drinks_id_drink",
                        column: x => x.id_drink,
                        principalTable: "drinks",
                        principalColumn: "id_drink",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favdrinks",
                columns: table => new
                {
                    id_user = table.Column<Guid>(type: "uuid", nullable: false),
                    id_drink = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favdrinks", x => new { x.id_user, x.id_drink });
                    table.ForeignKey(
                        name: "FK_favdrinks_drinks_id_drink",
                        column: x => x.id_drink,
                        principalTable: "drinks",
                        principalColumn: "id_drink",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favdrinks_users_id_user",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favcoffeeshops",
                columns: table => new
                {
                    id_user = table.Column<Guid>(type: "uuid", nullable: false),
                    id_coffeeshop = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favcoffeeshops", x => new { x.id_user, x.id_coffeeshop });
                    table.ForeignKey(
                        name: "FK_favcoffeeshops_coffeeshops_id_coffeeshop",
                        column: x => x.id_coffeeshop,
                        principalTable: "coffeeshops",
                        principalColumn: "id_coffeeshop",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favcoffeeshops_users_id_user",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coffeeshops_id_company",
                table: "coffeeshops",
                column: "id_company");

            migrationBuilder.CreateIndex(
                name: "IX_companies_id_lp",
                table: "companies",
                column: "id_lp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drinkscategory_id_category",
                table: "drinkscategory",
                column: "id_category");

            migrationBuilder.CreateIndex(
                name: "IX_favcoffeeshops_id_coffeeshop",
                table: "favcoffeeshops",
                column: "id_coffeeshop");

            migrationBuilder.CreateIndex(
                name: "IX_favdrinks_id_drink",
                table: "favdrinks",
                column: "id_drink");

            migrationBuilder.CreateIndex(
                name: "IX_menu_id_company",
                table: "menu",
                column: "id_company");

            migrationBuilder.CreateIndex(
                name: "IX_users_id_role",
                table: "users",
                column: "id_role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drinkscategory");

            migrationBuilder.DropTable(
                name: "favcoffeeshops");

            migrationBuilder.DropTable(
                name: "favdrinks");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "coffeeshops");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "drinks");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "loyaltyprograms");
        }
    }
}
