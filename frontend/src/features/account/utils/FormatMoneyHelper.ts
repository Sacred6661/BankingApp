export const formatMoney = (value: number, localString: string = "uk-UA") =>
  value.toLocaleString(localString, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

export const formatMoneyWithCurrency = (
  value: number,
  currency: string = "UAH",
  localString: string = "uk-UA",
) => {
  return formatMoney(value, localString) + " " + currency;
};
