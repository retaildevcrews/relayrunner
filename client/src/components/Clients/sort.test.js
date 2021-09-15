import { sortByProperty } from ".";

const testClients = [
  {
    id: 1,
    dateCreated: 2018,
  },
  {
    id: 2,
    dateCreated: 2020,
  },
  {
    id: 3,
    dateCreated: 2016,
  },
];

const sortedByDateClients = [
  {
    id: 3,
    dateCreated: 2016,
  },
  {
    id: 1,
    dateCreated: 2018,
  },
  {
    id: 2,
    dateCreated: 2020,
  },
];

test("sorting function sorts properly", () => {
  const sortedById = testClients.sort(sortByProperty("id"));
  const sortedByDate = testClients.sort(sortByProperty("dateCreated"));
  expect(sortedById).toStrictEqual(testClients);
  expect(sortedByDate).toStrictEqual(sortedByDateClients);
});
